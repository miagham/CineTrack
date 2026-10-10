import {spawn} from 'node:child_process';
import {resolve} from 'node:path';
const chrome=process.env.CHROME_PATH || 'C:/Program Files/Google/Chrome/Application/chrome.exe';
const proc=spawn(chrome,['--headless=new','--no-sandbox','--remote-debugging-port=9229',`--user-data-dir=${resolve('artifacts/browser-profile')}`,'about:blank'],{stdio:'ignore'});
const pause=ms=>new Promise(r=>setTimeout(r,ms));
let ws;
try {
 let page;
 for(let i=0;i<60;i++){try{page=(await (await fetch('http://127.0.0.1:9229/json')).json()).find(p=>p.type==='page');if(page)break;}catch{} await pause(250);}
 if(!page) throw Error('Chrome did not start');
 ws=new WebSocket(page.webSocketDebuggerUrl); await new Promise((r,j)=>{ws.onopen=r;ws.onerror=j;});
 let next=0; const requests=new Map(); const errors=[];
 ws.onmessage=e=>{const v=JSON.parse(e.data);if(v.id){const p=requests.get(v.id);requests.delete(v.id);v.error?p.reject(Error(v.error.message)):p.resolve(v.result);}else if(v.method==='Runtime.exceptionThrown')errors.push(v.params.exceptionDetails.text);};
 const call=(method,params={})=>new Promise((resolve,reject)=>{const id=++next;requests.set(id,{resolve,reject});ws.send(JSON.stringify({id,method,params}));});
 const evaluate=async expression=>{const r=await call('Runtime.evaluate',{expression,returnByValue:true,awaitPromise:true});if(r.exceptionDetails)throw Error(JSON.stringify(r.exceptionDetails));return r.result.value;};
 const until=async (expression,label)=>{for(let i=0;i<80;i++){if(await evaluate(expression))return;await pause(150);}throw Error(`Timed out: ${label}. Body: ${await evaluate('document.body.innerText')}`);};
 const go=async path=>{await call('Page.navigate',{url:`${process.env.BASE_URL || 'http://127.0.0.1:5088'}${path}`});await until('!!document.querySelector("main h1")',path);};
 const check=async(expression,label)=>{await until(expression,label);console.log(`PASS ${label}`);};
 const click=label=>evaluate(`Array.from(document.querySelectorAll('button')).find(b=>b.textContent.trim()===${JSON.stringify(label)}).click()`);
 const search=async value=>{await evaluate(`document.querySelector('#query').value=${JSON.stringify(value)};document.querySelector('#query').dispatchEvent(new Event('change',{bubbles:true}));document.querySelector('form').requestSubmit()`);await pause(300);await until("!document.body.innerText.includes('Searching...') && (document.querySelector('main h2')?.textContent.includes('results') || document.body.innerText.includes('Enter a title'))",'search results');};
 await call('Runtime.enable');
 await go('/'); await evaluate('localStorage.clear()'); await go('/');
 await check('document.querySelectorAll(".movie-card").length >= 6','Discover loads live trending and highly rated movies');
 await evaluate('document.querySelector(".movie-card img").scrollIntoView()');
 await check('document.querySelector(".movie-card img")?.complete && document.querySelector(".movie-card img").naturalWidth > 0','Discover poster image actually loads');
 await go('/search');await search('arrival');await check('document.querySelector(".movie-card h3")?.textContent === "Arrival"','title search');
 await evaluate('document.querySelector(".movie-card img").scrollIntoView()');
 await check('document.querySelector(".movie-card img")?.complete && document.querySelector(".movie-card img").naturalWidth > 0','search poster image actually loads');
 await click('+ Watchlist');await check('document.querySelector(".movie-card button")?.disabled','save movie');
 await go('/watchlist');await check('document.querySelector(".collection-item h2")?.textContent === "Arrival"','watchlist contains saved movie');
 await click('Set priority');await check(`Array.from(document.querySelectorAll("button")).some(b=>b.textContent === "High priority")`,'priority toggle');
 await click('Mark watched');await check('document.body.innerText.includes("Mark unwatched")','watched toggle');
 await go('/movie/tmdb-329865');await evaluate('const r=document.querySelector("#rating");r.value="4.5";r.dispatchEvent(new Event("change",{bubbles:true}))');await pause(400);
 await go('/my-movies');await check('document.body.innerText.includes("Your rating: 4.5")','rating and watched history persisted after navigation');
 await go('/profile');await check('document.querySelector(".stats strong")?.textContent === "1"','profile watched statistic');
 await go('/watchlist');await click('Remove from watchlist');await check('!document.querySelector(".collection-item")','watchlist removal');
 await go('/my-movies');await check('document.body.innerText.includes("Arrival")','removal preserves watched history');await click('Mark unwatched');await check('!document.querySelector(".collection-item")','unwatch removes watched history');
 await go('/genres');await until('Array.from(document.querySelectorAll("#genre option")).some(o=>o.textContent === "Horror")','API genres');
 await evaluate('const g=document.querySelector("#genre");g.value=Array.from(g.options).find(o=>o.textContent === "Horror").value;g.dispatchEvent(new Event("change",{bubbles:true}))');
 await pause(300);await check('!document.body.innerText.includes("Loading movies...") && document.querySelectorAll(".movie-card").length > 0 && Array.from(document.querySelectorAll(".movie-card")).every(p=>p.dataset.genres.split("|").includes("Horror"))','genre filter loads TMDB movies');
 await go('/search');await search('Denis Villeneuve');await check('document.querySelectorAll(".movie-card").length >= 3','director search');
 await search('under 2 hours');await check('document.querySelectorAll(".movie-card").length > 0','runtime phrase search');
 await search('zzzxqnomovie');await check('document.body.innerText.includes("No movies found")','no-results state');
 await search('');await check('document.body.innerText.includes("Enter a title")','empty search prompt');
  {
  await search('The Matrix');await check(`!!document.querySelector('a[href="/movie/tmdb-603"]')`,'live TMDB movie result');
  await go('/movie/tmdb-603');await check('document.querySelector("h1").textContent === "The Matrix" && !!document.querySelector(".detail-art img")','live TMDB details and poster');
  await check(`!!document.querySelector('a[href^="https://www.youtube.com/watch"]')`,'live trailer link');
  await click('Add to watchlist');await until('localStorage.getItem("cinetrack-v1")?.includes("tmdb-603")','online movie saved');await go('/watchlist');await check('document.body.innerText.includes("The Matrix")','online movie survives reload');
 }
 await go('/movie/missing');await check('document.querySelector("h1").textContent === "Movie not found"','missing movie state');
 await go('/about');await check('document.body.innerText.includes("TMDB")','about page');
 await call('Emulation.setDeviceMetricsOverride',{width:390,height:844,deviceScaleFactor:1,mobile:true});await go('/');await check('document.documentElement.scrollWidth <= 390','mobile layout does not overflow');
 if(errors.length)throw Error(`Browser errors: ${errors.join(', ')}`);
 console.log('All browser feature checks passed.');
} finally {ws?.close();proc.kill();}
