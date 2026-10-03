# CineTrack — Build Specification

> **Audience:** CineTrack team.
> **Source of truth:** four approved mockups (Home, Discover, Watchlist, Style Guide). This document translates them into rules, tokens, components, data, and behavior. Where the mockups conflict with each other, the **Resolved Decisions** section wins.

---

## 1. What CineTrack Is

**CineTrack: Personal Movie Companion** is a personal movie-tracking web app. Users browse movies, save them to a watchlist, mark them watched, rate them, and see stats about their viewing habits.

**Core loop:** Discover → Add to Watchlist → Mark Watched → Rate → See Stats.

**Key constraint:** all user data is stored locally on the device (footer copy: *"Your lists stay on your device."*). No accounts, no server-side user data.

**Visual concept:** *Letterboxd meets a classic movie theater, redesigned as a premium modern app.* Deep red velvet atmosphere, gold accents, cream text, editorial serif headings.

---

## 2. The One Rule (read this first)

> **Atmosphere belongs in the background. Information belongs in the foreground.**

Every design decision follows from this:

- Theater elements (red curtain texture, film-strip borders, glow) live **only** in page background and decorative bands. They never sit behind text at low contrast.
- Content (titles, ratings, buttons, stats) sits on **dark, solid panels** (`Near Black` / `Dark Burgundy`) with **cream text**. Readability always beats mood.
- Decorative typography (Playfair Display) is **headings only**. Body, labels, buttons, metadata use Manrope.
- The **search bar is the brightest element on every screen** (cream fill on dark). Nothing else should compete with it for brightness.
- Gold means **"active / important / actionable."** Don't use it as decoration.

If you're unsure about a choice, ask: *does this make information easier to read, or does it just add mood?* Mood goes in the background.

---

## 3. Design Tokens

### 3.1 Color

| Token | Hex | Use |
|---|---|---|
| `--color-deep-red` | `#5A0B12` | Page background (with subtle curtain/texture gradient) |
| `--color-dark-burgundy` | `#35070B` | Secondary surfaces, pills, inactive chips, header bar |
| `--color-near-black` | `#120607` | Cards and panels (primary content surfaces) |
| `--color-cream` | `#F5F2EA` | Primary text, search bar fill, inputs, secondary button fill |
| `--color-gold` | `#C9A227` | Active states, ratings stars, primary CTAs, eyebrows, selected chips |
| `--color-muted-gold` | `#8F741B` | Borders, hover depth, subtle outlines, progress-bar tracks |

Supporting (derive, don't invent new hues):
- Muted text: cream at ~60–70% opacity (metadata like `2024 · Sci-Fi`).
- Panel borders: `muted-gold` at low opacity, or cream at ~8–10% opacity.
- Text on gold: `near-black`.
- Text on cream: `near-black`.

```css
:root {
  --color-deep-red: #5A0B12;
  --color-dark-burgundy: #35070B;
  --color-near-black: #120607;
  --color-cream: #F5F2EA;
  --color-gold: #C9A227;
  --color-muted-gold: #8F741B;

  --text-primary: var(--color-cream);
  --text-muted: rgba(245, 242, 234, 0.65);
  --surface-page: var(--color-deep-red);
  --surface-secondary: var(--color-dark-burgundy);
  --surface-card: var(--color-near-black);
  --accent: var(--color-gold);
  --border-subtle: rgba(143, 116, 27, 0.35);

  --radius-card: 16px;
  --radius-poster: 10px;
  --radius-pill: 999px;
}
```

### 3.2 Typography

Fonts: **Playfair Display** (headings) and **Manrope** (everything else), both from Google Fonts.

| Role | Font | Weight | Size | Notes |
|---|---|---|---|---|
| Display (hero title) | Playfair Display | 800 | 92px (desktop) | e.g. "Arrival" in the hero |
| Page title | Playfair Display | 800 | ~56–64px | "Browse by Genre", "My Watchlist" |
| Section heading | Playfair Display | 800 | 36px | "Trending Movies", "Your Movie Stats" |
| Panel heading | Playfair Display | 800 | ~22–26px | "Recent History", "Filters", "Watchlist Stats" |
| Stat value | Playfair Display | 800 | ~28–32px | "110 hrs", "58", "12h 5m" |
| Eyebrow | Manrope | 800 | 13px, uppercase, letter-spacing ~0.12em, gold | "RECOMMENDED FOR YOU", "NOW SHOWING" |
| Card title | Manrope | 800 | 16–20px | Movie names under posters, list items |
| Body | Manrope | 500 | 18px / line-height 1.6 | Descriptions, subtitles |
| Metadata | Manrope | 500–600 | 13–14px, muted | `2023 · Drama · 2h 31m` |
| Buttons / chips | Manrope | 700–800 | 13–15px | |

Metadata format everywhere: **`YEAR · GENRE`** or **`YEAR · GENRE · RUNTIME`** separated by a middle dot with spaces.

Runtime format: `2h 46m` (never `166 min` in the UI). Time totals: `12h 5m`, hours stat: `110 hrs`.

### 3.3 Texture & Decoration

- **Page background:** deep red with a subtle vertical curtain-fold gradient and faint fine-grain/crosshatch texture. Very low contrast.
- **Film-strip edges:** thin dashed/sprocket-hole strips along the far left and right page edges.
- **Film-strip footer band:** a horizontal sprocket-hole band separating content from the footer.
- Decoration never overlaps content panels.

---

## 4. Global Layout

- Max content width ~1200px, centered. Generous vertical spacing between sections (~64–80px).
- Sections follow the pattern: **Eyebrow (gold, uppercase) → Heading (Playfair) → optional right-aligned "See all ›" / "View all ›" link → content.**
- Cards/panels: `near-black` fill, `--radius-card`, subtle border, soft shadow.

### 4.1 Header (all pages)

Left to right:
1. **Logo:** film-reel icon (gold) + wordmark "Cine" (cream) + "Track" (gold), Playfair Display.
2. **Search bar** (on every page except Home, where the hero search section replaces it; Home shows a search icon button instead). Cream pill, magnifier icon, placeholder `Search movies, actors, directors…`, near-black "Search" button inside the pill.
3. **Nav links:** `Home`, `Discover`, `My Movies`, `Watchlist`. Active link is gold with a short gold underline bar.
4. **Profile button:** outlined pill with user icon + "Profile".

### 4.2 Footer (all pages)

- Film-strip band on top.
- Left: wordmark + tagline *"Your personal movie companion. Your lists stay on your device."*
- Right: links `Home · Discover · My Movies · Watchlist · About` and `© 2026 CineTrack`.

---

## 5. Component Library

Build these as reusable components. Every screen is composed from them.

### 5.1 Buttons
| Variant | Style | Example |
|---|---|---|
| Primary | Gold fill, near-black text, pill, optional leading icon | `+ Add to Watchlist`, `Apply filters`, `+ Find movies to add` |
| Secondary | Transparent, cream 1px outline, cream text, pill | `▶ Trailer` / `Watch Trailer`, `Reset` |
| Tertiary | Dark burgundy/near-black fill, subtle border, cream text | `✓ Already watched` |
| Light action | Cream fill, near-black text, pill | `✓ Mark watched` (watchlist cards) |
| Done state | Transparent, gold outline + gold text | `✓ Watched` (after marking) |
| Icon button (circle) | Dark fill, subtle border | star (priority), trash (remove), carousel `‹ ›` |
| Icon button active | Gold outline + gold filled icon | star when high priority; grid/list toggle when selected |

### 5.2 Search Bar
Cream pill, the brightest element on screen. Magnifier icon + input + optional `Filters` pill (cream-tinted outline with sliders icon) + near-black `Search` button. On Home it's large (full content width); in the header it's compact.

### 5.3 Chips / Pills
- **Suggestion chips** (Home, under search): `Try:` label then near-black pills: `Christopher Nolan`, `Sci-Fi under 2 hours`, `Best Picture winners`, `A24`, `Denis Villeneuve`. Clicking one runs that search.
- **Genre chips** (Discover): dark burgundy pills; selected = gold fill, near-black text.
- **Filter tabs** (Watchlist): `All 8`, `Unwatched 5`, `Watched 3`, `High priority 3` — pill with a small count badge; selected = gold.

### 5.4 Status & Priority Tags
Small pills, ~11–12px bold text:
- **Watched** — gold fill, near-black text.
- **Want to watch** / **Unwatched** — near-black fill, cream text, thin cream/subtle outline.
- **★ High priority** — gold fill with star icon.
- **Section badges** — `SELECTED`, `POPULAR`: tiny uppercase gold text in a muted-gold outlined pill next to a section title.
- **Rating** — `★ 4.7`: gold star + value.

### 5.5 Movie Poster Card (vertical)
- Poster with ~2:3 aspect ratio, `--radius-poster`.
- Bottom gradient overlay on the poster with the **title in Playfair** (white) at bottom-left.
- Optional status tag top-left (`Watched` / `Want to watch`).
- Below poster: title (Manrope 800), then row: `YEAR · GENRE` (muted, left) and `★ rating` (gold, right).
- **Hover state:** card lifts (translateY ~-4px), gold glow/border, poster zooms slightly (~1.05), dark overlay fades in showing runtime + genre (`2h 46m · Sci-Fi`) and two buttons: `+ Watchlist` (primary) and `View details` (secondary).
- Placeholder posters: until real poster images are wired in, use a tinted vertical gradient per movie (as the mockups do) with the title overlaid. Real images should drop into the same slot.

### 5.6 Horizontal List Card (Watchlist)
Near-black card, small poster on the left, content on the right:
- Tag row (`★ High priority`, `Unwatched`/`Watched`)
- Title (Manrope 800, ~20px)
- `YEAR · GENRE · RUNTIME`
- `Added 2 days ago` (muted, relative time)
- Action row: `✓ Mark watched` (light) or `✓ Watched` (done state), star button, trash button.

### 5.7 Compact List Row
Small poster thumbnail + title + `YEAR · GENRE` + trailing gold star. Used in Home's "My Watchlist" panel and the "High Priority Picks" sidebar (the latter adds runtime and a `★ High priority` gold label).

### 5.8 Stat Card
Near-black card, square icon tile (muted-gold outline, gold icon) on the left, small muted label above a Playfair stat value. Used for: Top genre, Hours watched, Movies watched, Want to watch, Total saved, High priority, Watched, Unwatched.

### 5.9 Progress Bar Row (Watchlist Stats)
Label left (`Drama`), count right (`3 movies`), full-width track below (dark, muted) with gold fill proportional to the **largest** category (largest = 100%).

### 5.10 Carousel Controls
`See all` text link + two circular icon buttons `‹` `›`. Scrolls the row horizontally.

---

## 6. Screens

### 6.1 Home

Top to bottom:

1. **Hero ("Recommended for you")** — large near-black panel with a cinematic backdrop fading from dark on the left to the image on the right.
   - Eyebrow: `★ RECOMMENDED FOR YOU`
   - Title (Display): `Arrival`
   - Meta row: `2016 · Sci-Fi / Drama · 1h 56m · [PG-13] · ★ 4.6` (PG-13 in a small outlined box)
   - Synopsis (body, max ~45ch width)
   - Buttons: `+ Add to Watchlist` (primary), `▶ Watch Trailer` (secondary), `✓ Already watched` (tertiary)
   - Carousel dots bottom-right (active dot = gold elongated pill). Hero rotates between several recommendations.
2. **Search section** — eyebrow `SEARCH`, heading `What are we watching tonight?`, large search bar with `Filters` and `Search`, suggestion chips row.
3. **Your Movie Stats** — eyebrow `YOUR YEAR IN FILM`, heading, `Full stats ›` link. Four stat cards: Top genre, Hours watched, Movies watched, Want to watch.
4. **Trending Movies** — eyebrow `NOW SHOWING`, heading, `See all` + carousel arrows. Row of 6 poster cards with status tags where the user has one.
5. **Two-column bottom row:**
   - **Recent History** (wider, ~2/3): `View all ›`. Last 3 watched movies as poster + title + `YEAR · GENRE` + `★ 4.5 your rating` + `🕒 2 days ago`.
   - **My Watchlist** (~1/3): `View all ›`. Top 3 watchlist items as compact rows.

### 6.2 Discover (Browse by Genre)

1. Eyebrow `DISCOVER`, title `Browse by Genre`, subtitle `Pick a genre, narrow it down, and add what catches your eye.`
2. Genre chip row: `Action, Comedy, Drama, Horror, Sci-Fi, Romance, Thriller, Animation, Documentary, Mystery`. One selected (gold).
3. Two-column layout:
   - **Left: Filters panel** (sticky, ~220px):
     - `RUNTIME` checkboxes: Under 90 min, 90–120 min, 120–180 min, Over 180 min
     - `RELEASE YEAR`: two selects `Any` to `Any`
     - `RATING` checkboxes: 1–3 stars, 3–4 stars, 4+ stars
     - `STATUS` checkboxes: Not watched yet, Hide my watchlist
     - `Apply filters` (primary, full width), `Reset` (secondary, full width)
     - Checked checkboxes: gold fill with near-black check.
     - Section labels are gold eyebrows; sections separated by thin dividers.
   - **Right: Genre rows** — each row is its own near-black panel: title (`Action Movies`) + badge (`SELECTED` for the chosen genre, `POPULAR` for others) + `See all` + arrows, then 5 poster cards.
   - The selected genre's row appears first, followed by other popular genre rows.

### 6.3 Watchlist

1. Eyebrow `YOUR QUEUE`, title `My Watchlist`, subtitle `Everything you've saved, with your must-sees up front.` Right-aligned primary button `+ Find movies to add` (→ Discover).
2. Two-column layout:
   - **Main column:**
     - Four stat cards: Total saved, High priority, Watched, Unwatched.
     - Toolbar: filter tabs (`All`, `Unwatched`, `Watched`, `High priority` with counts) on the left; on the right a sort dropdown (`Recently added` default) and grid/list view toggle.
     - 2-column grid of horizontal list cards (list view = single column).
     - **Default ordering:** high-priority unwatched first, then other unwatched, then watched — each group by recently added. ("Must-sees up front.")
   - **Sidebar:**
     - **High Priority Picks** panel (`View all`): compact rows with runtime + `★ High priority`.
     - **Watchlist Stats** panel: genre progress bars, then a highlighted box: 🕒 `Time to clear your list` → `12h 5m`.

### 6.4 Screens not yet mocked
`My Movies`, `Profile`, `About`, movie details (from `View details`), and `Full stats` are linked but not designed. Build them using the same components and section pattern. Suggested scope:
- **My Movies:** all watched movies with the user's ratings, sortable; essentially an expanded Recent History.
- **Movie details:** hero-style header (like Home's hero) + synopsis, runtime, genre, community rating, the user's rating control, and the three hero buttons.

---

## 7. Data Model

All user data persists locally (e.g. `localStorage`/IndexedDB), namespaced under a single key/version so it can be migrated.

```ts
type Genre =
  | "Action" | "Comedy" | "Drama" | "Horror" | "Sci-Fi"
  | "Romance" | "Thriller" | "Animation" | "Documentary" | "Mystery";

interface Movie {              // catalog data (seed JSON or external API)
  id: string;
  title: string;
  year: number;
  genres: Genre[];             // first entry = primary genre, used in "YEAR · GENRE"
  runtimeMin: number;          // store minutes, format as "2h 46m" in UI
  certification?: string;      // "PG-13"
  communityRating: number;     // 0–5, one decimal
  synopsis: string;
  posterUrl?: string;          // optional; fall back to gradient placeholder
  backdropUrl?: string;
  trailerUrl?: string;
  people?: string[];           // directors/actors, for search
}

type Status = "want_to_watch" | "watched";

interface UserMovieEntry {     // user data, stored locally
  movieId: string;
  status: Status;
  onWatchlist: boolean;        // stays true after watching unless removed
  highPriority: boolean;
  userRating?: number;         // 0.5–5, set when watched
  addedAt: string;             // ISO date → "Added 2 days ago"
  watchedAt?: string;          // ISO date → "2 days ago" in Recent History
}
```

### 7.1 Derived values (compute, never store)

| Value | Rule |
|---|---|
| Total saved | count of entries with `onWatchlist` |
| Watched / Unwatched (watchlist) | split of watchlist entries by `status` |
| High priority | watchlist entries with `highPriority` |
| Time to clear your list | sum of `runtimeMin` for **unwatched** watchlist entries, formatted `Xh Ym` |
| Watchlist Stats bars | watchlist entries grouped by primary genre; bar width = count ÷ max count |
| Movies watched (Home) | all entries with `status = watched` |
| Hours watched | sum of watched runtimes ÷ 60, rounded, `N hrs` |
| Top genre | most frequent primary genre among watched movies |
| Want to watch (Home) | entries with `status = want_to_watch` |
| Recent History | watched entries sorted by `watchedAt` desc, top 3 |

Sanity check against the mockup data: unwatched runtimes 166 + 151 + 164 + 111 + 133 = 725 min = **12h 5m**. Your implementation should reproduce this with the seed data below.

---

## 8. Behavior & State Rules

- **Add to Watchlist:** creates an entry (`want_to_watch`, `onWatchlist: true`, `addedAt: now`). Button changes to a done state (`✓ On watchlist`). Toast confirmation.
- **Already watched / Mark watched:** sets `status: watched`, `watchedAt: now`, then prompts for a star rating (skippable). Button becomes `✓ Watched` (gold outline). Stats update immediately.
- **Star button (watchlist):** toggles `highPriority`. Active = gold.
- **Trash button:** removes from watchlist, with an Undo toast (no confirm dialog).
- **Status tags on posters** always reflect the user's data across every screen (Trending, Discover, etc.).
- **Search:** matches title, people, and genre. Suggestion chips pre-fill queries (e.g. "Sci-Fi under 2 hours" = genre Sci-Fi + runtime < 120).
- **Discover filters:** apply on `Apply filters`; `Reset` clears all. "Not watched yet" hides watched; "Hide my watchlist" hides anything on the watchlist.
- **Hero:** auto-rotates recommendations (pause on hover/focus); dots are clickable. Recommendation = highly rated, unwatched, not on watchlist, weighted toward the user's top genre.
- **Empty states:** every list needs one (e.g. empty watchlist → message + `Find movies to add` button). New users see zeroed stats, not broken layouts.

---

## 9. Responsive Behavior

- **Desktop (≥1100px):** layouts as mocked.
- **Tablet (700–1099px):** Discover filters collapse into a `Filters` button opening a drawer; Watchlist sidebar moves below the main column; stat cards 2×2.
- **Mobile (<700px):** header collapses to logo + search icon + menu; poster rows become horizontally swipeable; watchlist cards single column; hero title scales down (~48px); 16px side gutters; no horizontal page scroll.

---

## 10. Accessibility & Quality Bar

- Text contrast ≥ 4.5:1. Cream on near-black passes; **never place cream body text directly on the textured deep-red background.**
- Gold (`#C9A227`) on near-black is fine for text/icons; don't use muted gold for text.
- All icon-only buttons need `aria-label` (e.g. "Mark as high priority", "Remove from watchlist").
- Visible focus rings (gold outline). Hover-only actions on poster cards must also be reachable by keyboard focus and by tap on touch devices.
- Respect `prefers-reduced-motion`: disable hero auto-rotate and card zoom.
- Status must not rely on color alone — tags always include text.

---

## 11. Seed Data (from the mockups)

Use this so the first build matches the mockups exactly.

**Catalog (community ratings as shown on Discover/Trending):**

| Title | Year | Primary genre | Runtime | Rating |
|---|---|---|---|---|
| Arrival | 2016 | Sci-Fi (also Drama) | 1h 56m | 4.6 |
| Dune: Part Two | 2024 | Sci-Fi | 2h 46m | 4.7 |
| Oppenheimer | 2023 | Drama | — | 4.6 |
| Past Lives | 2023 | Drama (also Romance) | 1h 46m | 4.5 |
| Everything Everywhere All at Once | 2022 | Sci-Fi | — | 4.5 |
| The Holdovers | 2023 | Comedy | 2h 13m | 4.4 |
| Anatomy of a Fall | 2023 | Drama | 2h 31m | 4.3 |
| Blade Runner 2049 | 2017 | Sci-Fi | 2h 44m | — |
| Moonlight | 2016 | Drama | 1h 51m | 4.5 |
| Mad Max: Fury Road | 2015 | Action | 2h 0m | 4.8 |
| John Wick | 2014 | Action | 1h 41m | 4.3 |
| Top Gun: Maverick | 2022 | Action | — | 4.5 |
| The Dark Knight | 2008 | Action | — | 4.8 |
| Mission: Impossible – Fallout | 2018 | Action | — | 4.4 |
| The Grand Budapest Hotel | 2014 | Comedy | — | 4.6 |
| Knives Out | 2019 | Comedy | — | 4.4 |
| Paddington 2 | 2017 | Comedy | — | 4.6 |
| Superbad | 2007 | Comedy | — | 4.0 |
| Whiplash | 2014 | Drama | — | 4.7 |
| Parasite | 2019 | Drama | — | 4.8 |

Fill "—" with accurate real-world values.

**User's watchlist (8):**

| Movie | Status | High priority | Added | User rating |
|---|---|---|---|---|
| Dune: Part Two | Unwatched | ★ | 2 days ago | — |
| Anatomy of a Fall | Unwatched | ★ | 4 days ago | — |
| Blade Runner 2049 | Unwatched | ★ | 1 week ago | — |
| Moonlight | Unwatched | | 1 week ago | — |
| The Holdovers | Unwatched | | 2 weeks ago | — |
| Past Lives | Watched (2 days ago) | | 3 weeks ago | 4.5 |
| Mad Max: Fury Road | Watched (5 days ago) | | 1 month ago | 5.0 |
| John Wick | Watched (1 week ago) | | 1 month ago | 4.0 |

Other watched (not on watchlist, per Discover tags): Superbad, Whiplash.

---

## 12. Resolved Decisions (mockup inconsistencies)

The mockups disagree in a few places. Implement these rules:

1. **Nav items:** Home shows a `Favorites` link; other pages don't. → Use **Home, Discover, My Movies, Watchlist** everywhere. Drop Favorites (high priority covers that need).
2. **Header search:** Home uses a search icon; other pages show the full bar. → Keep as mocked: icon on Home (the hero search section is right there), full bar elsewhere.
3. **Past Lives genre:** labeled Romance on Home, Drama elsewhere. → Primary genre **Drama**, secondary Romance. Always display the primary genre.
4. **"Want to watch 8" (Home) vs "Unwatched 5" (Watchlist):** → Home stat = count of `want_to_watch` entries. Both numbers come from the same derived logic; don't hardcode.
5. **Rating vs your rating:** poster cards show the **community rating**; Recent History and My Movies show **your rating** labeled "your rating." Never mix them without a label.
6. **Status tag wording:** poster cards say `Want to watch`; watchlist cards say `Unwatched`. Same underlying state (`want_to_watch`), keep both labels as mocked.

---

## 13. Acceptance Checklist

- [ ] Palette, fonts, and sizes match Sections 3–5; only the six tokens are used for color.
- [ ] Search bar is the brightest element on every page.
- [ ] No body text sits directly on the textured red background.
- [ ] Home, Discover, and Watchlist match the mockups at desktop width.
- [ ] Seed data reproduces the mockup numbers (8 saved / 3 high priority / 3 watched / 5 unwatched / 12h 5m; Drama 3, Sci-Fi 2, Action 2, Comedy 1).
- [ ] Adding, watching, rating, prioritizing, and removing movies update every screen and stat instantly.
- [ ] Data survives a page reload (local persistence).
- [ ] Poster hover actions work by keyboard and touch.
- [ ] Layout works at 375px width with no horizontal scroll.
- [ ] Empty states exist for every list.
