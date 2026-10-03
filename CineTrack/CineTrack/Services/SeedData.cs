using CineTrack.Models;

namespace CineTrack.Services
{
    /// <summary>
    /// Sample movies and starter user data for testing,
    /// plus a few extra titles so every genre on Discover has something to show.
    /// </summary>
    public static class SeedData
    {
        public static readonly string[] TrendingIds =
        {
            "dune-part-two", "oppenheimer", "past-lives", "everything-everywhere",
            "the-holdovers", "anatomy-of-a-fall"
        };

        public static List<Movie> Catalog() => new()
        {
            M("arrival", "Arrival", 2016, new[] { "Sci-Fi", "Drama" }, 116, "PG-13", 4.6, "#3E4A57",
              "When twelve mysterious spacecraft touch down around the world, a linguist is recruited to find out why they came. What she learns about their language rewrites her understanding of time itself.",
              new[] { "Denis Villeneuve", "Amy Adams", "Jeremy Renner" }, editorsPick: true),
            M("dune-part-two", "Dune: Part Two", 2024, new[] { "Sci-Fi", "Action" }, 166, "PG-13", 4.7, "#B07A38",
              "Paul Atreides unites with the Fremen on a path of revenge against the conspirators who destroyed his family, while visions warn him of the war his choices could unleash.",
              new[] { "Denis Villeneuve", "Timothée Chalamet", "Zendaya", "Rebecca Ferguson" }),
            M("oppenheimer", "Oppenheimer", 2023, new[] { "Drama", "Thriller" }, 180, "R", 4.6, "#8A4B1E",
              "The story of J. Robert Oppenheimer and the race to build the atomic bomb — and the reckoning that followed the man who helped change the world forever.",
              new[] { "Christopher Nolan", "Cillian Murphy", "Emily Blunt", "Robert Downey Jr." }, "Best Picture winner"),
            M("past-lives", "Past Lives", 2023, new[] { "Drama", "Romance" }, 106, "PG-13", 4.5, "#4F6B78",
              "Two deeply connected childhood friends are separated when one emigrates from Seoul. Decades later they reunite in New York for one fateful week.",
              new[] { "Celine Song", "Greta Lee", "Teo Yoo", "John Magaro" }, "A24"),
            M("everything-everywhere", "Everything Everywhere All at Once", 2022, new[] { "Sci-Fi", "Comedy", "Action" }, 139, "R", 4.5, "#6B3F7A",
              "An exhausted laundromat owner is swept into a multiverse adventure where only she can save existence — by connecting with the lives she could have led.",
              new[] { "Daniel Kwan", "Daniel Scheinert", "Michelle Yeoh", "Ke Huy Quan", "Jamie Lee Curtis" }, "A24", "Best Picture winner"),
            M("the-holdovers", "The Holdovers", 2023, new[] { "Comedy", "Drama" }, 133, "R", 4.4, "#6E5A3A",
              "A curmudgeonly teacher is stuck on campus over the holidays with a troubled student and the school's grieving head cook, forming an unlikely bond.",
              new[] { "Alexander Payne", "Paul Giamatti", "Da'Vine Joy Randolph", "Dominic Sessa" }),
            M("anatomy-of-a-fall", "Anatomy of a Fall", 2023, new[] { "Drama", "Mystery" }, 151, "R", 4.3, "#56606A",
              "A writer is put on trial for her husband's death, and their son is left to decide what he really believes about the marriage he grew up inside.",
              new[] { "Justine Triet", "Sandra Hüller", "Milo Machado-Graner" }),
            M("blade-runner-2049", "Blade Runner 2049", 2017, new[] { "Sci-Fi", "Thriller" }, 164, "R", 4.4, "#A34F24",
              "A young blade runner unearths a long-buried secret that leads him to track down a former blade runner who has been missing for thirty years.",
              new[] { "Denis Villeneuve", "Ryan Gosling", "Harrison Ford", "Ana de Armas" }),
            M("moonlight", "Moonlight", 2016, new[] { "Drama" }, 111, "R", 4.5, "#2B4C8C",
              "Three chapters in the life of a young man growing up in Miami, as he struggles to find himself and his place in the world.",
              new[] { "Barry Jenkins", "Mahershala Ali", "Trevante Rhodes", "Naomie Harris" }, "A24", "Best Picture winner"),
            M("mad-max-fury-road", "Mad Max: Fury Road", 2015, new[] { "Action", "Sci-Fi" }, 120, "R", 4.8, "#A8472A",
              "In a post-apocalyptic wasteland, a drifter and a rebel warrior race across the desert to free a tyrant's captives in a relentless two-hour chase.",
              new[] { "George Miller", "Tom Hardy", "Charlize Theron", "Nicholas Hoult" }),
            M("john-wick", "John Wick", 2014, new[] { "Action", "Thriller" }, 101, "R", 4.3, "#2F3A4F",
              "A retired hitman is pulled back into the underworld he left behind when gangsters take the last thing his late wife gave him.",
              new[] { "Chad Stahelski", "Keanu Reeves", "Willem Dafoe", "Ian McShane" }),
            M("top-gun-maverick", "Top Gun: Maverick", 2022, new[] { "Action", "Drama" }, 131, "PG-13", 4.5, "#5B7A99",
              "After thirty years of service, Maverick is called back to train a group of elite graduates for a mission no living pilot has ever seen.",
              new[] { "Joseph Kosinski", "Tom Cruise", "Miles Teller", "Jennifer Connelly" }),
            M("the-dark-knight", "The Dark Knight", 2008, new[] { "Action", "Thriller" }, 152, "PG-13", 4.8, "#2A2F3A",
              "Batman faces the Joker, a criminal mastermind who plunges Gotham into anarchy and forces the hero to question everything he stands for.",
              new[] { "Christopher Nolan", "Christian Bale", "Heath Ledger", "Aaron Eckhart" }),
            M("mission-impossible-fallout", "Mission: Impossible – Fallout", 2018, new[] { "Action", "Thriller" }, 147, "PG-13", 4.4, "#4B5B3F",
              "Ethan Hunt and his team race against time after a mission goes wrong, with the CIA watching their every move.",
              new[] { "Christopher McQuarrie", "Tom Cruise", "Henry Cavill", "Rebecca Ferguson" }),
            M("grand-budapest-hotel", "The Grand Budapest Hotel", 2014, new[] { "Comedy" }, 99, "R", 4.6, "#B06A7C",
              "A legendary concierge and his loyal lobby boy are caught up in the theft of a priceless painting and a battle for an enormous family fortune.",
              new[] { "Wes Anderson", "Ralph Fiennes", "Tony Revolori", "Saoirse Ronan" }),
            M("knives-out", "Knives Out", 2019, new[] { "Comedy", "Mystery" }, 130, "PG-13", 4.4, "#6A3B2A",
              "When a renowned crime novelist dies the night of his 85th birthday, an inquisitive detective investigates his wonderfully dysfunctional family.",
              new[] { "Rian Johnson", "Daniel Craig", "Ana de Armas", "Chris Evans" }),
            M("paddington-2", "Paddington 2", 2017, new[] { "Comedy" }, 103, "PG", 4.6, "#3F6B8A",
              "Paddington picks up odd jobs to buy the perfect present for his aunt's birthday — until the gift is stolen and he is wrongly accused.",
              new[] { "Paul King", "Ben Whishaw", "Hugh Grant", "Sally Hawkins" }),
            M("superbad", "Superbad", 2007, new[] { "Comedy" }, 113, "R", 4.0, "#6B7F2E",
              "Two co-dependent high school seniors try to make the most of their last weeks before graduation with one night of disastrous party plans.",
              new[] { "Greg Mottola", "Jonah Hill", "Michael Cera", "Emma Stone" }),
            M("whiplash", "Whiplash", 2014, new[] { "Drama" }, 106, "R", 4.7, "#8C6A1E",
              "A promising young drummer enrolls at a cutthroat music conservatory, where an instructor will stop at nothing to push him past his limits.",
              new[] { "Damien Chazelle", "Miles Teller", "J.K. Simmons" }),
            M("parasite", "Parasite", 2019, new[] { "Drama", "Thriller" }, 132, "R", 4.8, "#3D5A45",
              "Greed and class discrimination threaten the newly formed symbiotic relationship between the wealthy Park family and the destitute Kim clan.",
              new[] { "Bong Joon-ho", "Song Kang-ho", "Cho Yeo-jeong", "Choi Woo-shik" }, "Best Picture winner"),

            // Extra titles so every genre row has something in it.
            M("get-out", "Get Out", 2017, new[] { "Horror", "Mystery" }, 104, "R", 4.3, "#3A4A3A",
              "A young man visits his girlfriend's family estate for the weekend, where their unsettling hospitality hides something far more sinister.",
              new[] { "Jordan Peele", "Daniel Kaluuya", "Allison Williams" }),
            M("hereditary", "Hereditary", 2018, new[] { "Horror" }, 127, "R", 4.1, "#5A3A2A",
              "After the family matriarch dies, her daughter's family begins to unravel cryptic and increasingly terrifying secrets about their ancestry.",
              new[] { "Ari Aster", "Toni Collette", "Alex Wolff" }, "A24"),
            M("before-sunrise", "Before Sunrise", 1995, new[] { "Romance", "Drama" }, 101, "R", 4.4, "#8A6A4A",
              "A young American and a French student meet on a train and spend one night walking and talking through Vienna before they must part.",
              new[] { "Richard Linklater", "Ethan Hawke", "Julie Delpy" }),
            M("la-la-land", "La La Land", 2016, new[] { "Romance", "Drama" }, 128, "PG-13", 4.3, "#3B3F8C",
              "A jazz pianist and an aspiring actress fall in love in Los Angeles while chasing dreams that begin to pull them apart.",
              new[] { "Damien Chazelle", "Ryan Gosling", "Emma Stone" }),
            M("prisoners", "Prisoners", 2013, new[] { "Thriller", "Mystery" }, 153, "R", 4.3, "#4A5058",
              "When his daughter and her friend go missing, a desperate father takes matters into his own hands as the police pursue every lead.",
              new[] { "Denis Villeneuve", "Hugh Jackman", "Jake Gyllenhaal" }),
            M("gone-girl", "Gone Girl", 2014, new[] { "Thriller", "Mystery" }, 149, "R", 4.2, "#5B6670",
              "On his fifth wedding anniversary, a man's wife disappears, and the media circus that follows makes him the prime suspect.",
              new[] { "David Fincher", "Rosamund Pike", "Ben Affleck" }),
            M("spirited-away", "Spirited Away", 2001, new[] { "Animation" }, 125, "PG", 4.7, "#3F7A6B",
              "A sullen ten-year-old wanders into a world of spirits, and must work in a bathhouse for the gods to free herself and her parents.",
              new[] { "Hayao Miyazaki", "Rumi Hiiragi", "Miyu Irino" }),
            M("spider-verse", "Spider-Man: Into the Spider-Verse", 2018, new[] { "Animation", "Action" }, 117, "PG", 4.6, "#8C2F5A",
              "Teenager Miles Morales becomes Spider-Man and joins Spider-People from other dimensions to stop a threat to every reality.",
              new[] { "Bob Persichetti", "Peter Ramsey", "Rodney Rothman", "Shameik Moore" }),
            M("free-solo", "Free Solo", 2018, new[] { "Documentary" }, 100, "PG-13", 4.3, "#7A6A52",
              "Rock climber Alex Honnold prepares to achieve his lifelong dream: climbing El Capitan in Yosemite without a rope.",
              new[] { "Elizabeth Chai Vasarhelyi", "Jimmy Chin", "Alex Honnold" }),
            M("wont-you-be-my-neighbor", "Won't You Be My Neighbor?", 2018, new[] { "Documentary" }, 94, "PG-13", 4.4, "#6B4A7A",
              "An intimate look at the life and legacy of Fred Rogers, whose gentle television show shaped generations of children.",
              new[] { "Morgan Neville", "Fred Rogers" }),
            M("glass-onion", "Glass Onion", 2022, new[] { "Mystery", "Comedy" }, 139, "PG-13", 3.9, "#3F6A7A",
              "Detective Benoit Blanc travels to a Greek island to peel back the layers of a mystery among a tech billionaire's circle of friends.",
              new[] { "Rian Johnson", "Daniel Craig", "Edward Norton", "Janelle Monáe" }),
            M("zodiac", "Zodiac", 2007, new[] { "Mystery", "Thriller" }, 157, "R", 4.2, "#4A4A3A",
              "A cartoonist becomes obsessed with tracking down the Zodiac Killer, the serial murderer who terrorized San Francisco.",
              new[] { "David Fincher", "Jake Gyllenhaal", "Mark Ruffalo", "Robert Downey Jr." }),
        };

        /// <summary>The starter watchlist, with dates relative to <paramref name="now"/>.</summary>
        public static List<UserMovieEntry> UserEntries(DateTime now) => new()
        {
            Want("dune-part-two", now.AddDays(-2), highPriority: true),
            Want("anatomy-of-a-fall", now.AddDays(-4), highPriority: true),
            Want("blade-runner-2049", now.AddDays(-7), highPriority: true),
            Want("moonlight", now.AddDays(-7).AddHours(-1)),
            Want("the-holdovers", now.AddDays(-14)),
            Watched("past-lives", added: now.AddDays(-21), watched: now.AddDays(-2), rating: 4.5, onWatchlist: true),
            Watched("mad-max-fury-road", added: now.AddDays(-30), watched: now.AddDays(-5), rating: 5.0, onWatchlist: true),
            Watched("john-wick", added: now.AddDays(-31), watched: now.AddDays(-7), rating: 4.0, onWatchlist: true),
            // Watched but not on the watchlist (tagged "Watched" on Discover).
            Watched("superbad", added: now.AddDays(-45), watched: now.AddDays(-21), rating: 4.0, onWatchlist: false),
            Watched("whiplash", added: now.AddDays(-60), watched: now.AddDays(-35), rating: 4.5, onWatchlist: false),
        };

        static Movie M(string id, string title, int year, string[] genres, int runtime, string cert,
                       double rating, string tint, string synopsis, string[] people,
                       params string[] keywords) =>
            M(id, title, year, genres, runtime, cert, rating, tint, synopsis, people, false, keywords);

        static Movie M(string id, string title, int year, string[] genres, int runtime, string cert,
                       double rating, string tint, string synopsis, string[] people,
                       bool editorsPick, params string[] keywords) => new()
        {
            Id = id,
            Title = title,
            Year = year,
            Genres = genres.ToList(),
            RuntimeMin = runtime,
            Certification = cert,
            CommunityRating = rating,
            Tint = tint,
            Synopsis = synopsis,
            People = people.ToList(),
            Keywords = keywords.ToList(),
            EditorsPick = editorsPick,
        };

        static UserMovieEntry Want(string id, DateTime added, bool highPriority = false) => new()
        {
            MovieId = id,
            Status = WatchStatus.WantToWatch,
            OnWatchlist = true,
            HighPriority = highPriority,
            AddedAt = added,
        };

        static UserMovieEntry Watched(string id, DateTime added, DateTime watched, double rating, bool onWatchlist) => new()
        {
            MovieId = id,
            Status = WatchStatus.Watched,
            OnWatchlist = onWatchlist,
            UserRating = rating,
            AddedAt = added,
            WatchedAt = watched,
        };
    }
}
