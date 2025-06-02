private static readonly List<Book> _books = new()
{
    new Book
    {
        Id = "1",
        Title = "The Great Gatsby",
        Author = "F. Scott Fitzgerald",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "2",
        Title = "To Kill a Mockingbird",
        Author = "Harper Lee",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    },
    new Book
    {
        Id = "3",
        Title = "1984",
        Author = "George Orwell",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
    },
    new Book
    {
        Id = "4",
        Title = "Pride and Prejudice",
        Author = "Jane Austen",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "5",
        Title = "The Catcher in the Rye",
        Author = "J.D. Salinger",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-6).ToString("o")
    },
    new Book
    {
        Id = "6",
        Title = "Brave New World",
        Author = "Aldous Huxley",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-1).ToString("o")
    },
    new Book
    {
        Id = "7",
        Title = "The Lord of the Rings",
        Author = "J.R.R. Tolkien",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "8",
        Title = "Animal Farm",
        Author = "George Orwell",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-8).ToString("o")
    },
    new Book
    {
        Id = "9",
        Title = "The Hobbit",
        Author = "J.R.R. Tolkien",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    },
    new Book
    {
        Id = "10",
        Title = "Moby-Dick",
        Author = "Herman Melville",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-9).ToString("o")
    },
    new Book
    {
        Id = "11",
        Title = "Wuthering Heights",
        Author = "Emily Brontë",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "12",
        Title = "The Odyssey",
        Author = "Homer",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "13",
        Title = "Jane Eyre",
        Author = "Charlotte Brontë",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
    },
    new Book
    {
        Id = "14",
        Title = "Frankenstein",
        Author = "Mary Shelley",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "15",
        Title = "The Picture of Dorian Gray",
        Author = "Oscar Wilde",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-6).ToString("o")
    },
    new Book
    {
        Id = "16",
        Title = "Crime and Punishment",
        Author = "Fyodor Dostoevsky",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    },
    new Book
    {
        Id = "17",
        Title = "The Brothers Karamazov",
        Author = "Fyodor Dostoevsky",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-8).ToString("o")
    },
    new Book
    {
        Id = "18",
        Title = "War and Peace",
        Author = "Leo Tolstoy",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "19",
        Title = "Anna Karenina",
        Author = "Leo Tolstoy",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "20",
        Title = "Don Quixote",
        Author = "Miguel de Cervantes",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
    },
    new Book
    {
        Id = "21",
        Title = "The Divine Comedy",
        Author = "Dante Alighieri",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "22",
        Title = "Les Misérables",
        Author = "Victor Hugo",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-9).ToString("o")
    },
    new Book
    {
        Id = "23",
        Title = "The Grapes of Wrath",
        Author = "John Steinbeck",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-6).ToString("o")
    },
    new Book
    {
        Id = "24",
        Title = "East of Eden",
        Author = "John Steinbeck",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "25",
        Title = "One Hundred Years of Solitude",
        Author = "Gabriel García Márquez",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "26",
        Title = "Love in the Time of Cholera",
        Author = "Gabriel García Márquez",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-8).ToString("o")
    },
    new Book
    {
        Id = "27",
        Title = "The Old Man and the Sea",
        Author = "Ernest Hemingway",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    },
    new Book
    {
        Id = "28",
        Title = "For Whom the Bell Tolls",
        Author = "Ernest Hemingway",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
    },
    new Book
    {
        Id = "29",
        Title = "A Farewell to Arms",
        Author = "Ernest Hemingway",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "30",
        Title = "The Sun Also Rises",
        Author = "Ernest Hemingway",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-6).ToString("o")
    },
    new Book
    {
        Id = "31",
        Title = "The Count of Monte Cristo",
        Author = "Alexandre Dumas",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "32",
        Title = "The Three Musketeers",
        Author = "Alexandre Dumas",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-9).ToString("o")
    },
    new Book
    {
        Id = "33",
        Title = "Ulysses",
        Author = "James Joyce",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "34",
        Title = "Dubliners",
        Author = "James Joyce",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    },
    new Book
    {
        Id = "35",
        Title = "Great Expectations",
        Author = "Charles Dickens",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-8).ToString("o")
    },
    new Book
    {
        Id = "36",
        Title = "David Copperfield",
        Author = "Charles Dickens",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "37",
        Title = "A Tale of Two Cities",
        Author = "Charles Dickens",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
    },
    new Book
    {
        Id = "38",
        Title = "Oliver Twist",
        Author = "Charles Dickens",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "39",
        Title = "Bleak House",
        Author = "Charles Dickens",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-6).ToString("o")
    },
    new Book
    {
        Id = "40",
        Title = "The Adventures of Huckleberry Finn",
        Author = "Mark Twain",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "41",
        Title = "The Adventures of Tom Sawyer",
        Author = "Mark Twain",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-9).ToString("o")
    },
    new Book
    {
        Id = "42",
        Title = "Catch-22",
        Author = "Joseph Heller",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "43",
        Title = "Slaughterhouse-Five",
        Author = "Kurt Vonnegut",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
    },
    new Book
    {
        Id = "44",
        Title = "The Handmaid's Tale",
        Author = "Margaret Atwood",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    },
    new Book
    {
        Id = "45",
        Title = "The Bell Jar",
        Author = "Sylvia Plath",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-8).ToString("o")
    },
    new Book
    {
        Id = "46",
        Title = "Mrs. Dalloway",
        Author = "Virginia Woolf",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "47",
        Title = "To the Lighthouse",
        Author = "Virginia Woolf",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "48",
        Title = "The Sound and the Fury",
        Author = "William Faulkner",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-6).ToString("o")
    },
    new Book
    {
        Id = "49",
        Title = "As I Lay Dying",
        Author = "William Faulkner",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "50",
        Title = "Heart of Darkness",
        Author = "Joseph Conrad",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-9).ToString("o")
    },
    new Book
    {
        Id = "51",
        Title = "Lord of the Flies",
        Author = "William Golding",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
    },
    new Book
    {
        Id = "52",
        Title = "The Alchemist",
        Author = "Paulo Coelho",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    },
    new Book
    {
        Id = "53",
        Title = "The Little Prince",
        Author = "Antoine de Saint-Exupéry",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "54",
        Title = "The Stranger",
        Author = "Albert Camus",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-8).ToString("o")
    },
    new Book
    {
        Id = "55",
        Title = "The Plague",
        Author = "Albert Camus",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "56",
        Title = "Invisible Man",
        Author = "Ralph Ellison",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-6).ToString("o")
    },
    new Book
    {
        Id = "57",
        Title = "Native Son",
        Author = "Richard Wright",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "58",
        Title = "Beloved",
        Author = "Toni Morrison",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-9).ToString("o")
    },
    new Book
    {
        Id = "59",
        Title = "Song of Solomon",
        Author = "Toni Morrison",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "60",
        Title = "Things Fall Apart",
        Author = "Chinua Achebe",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    },
    new Book
    {
        Id = "61",
        Title = "The Color Purple",
        Author = "Alice Walker",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
    },
    new Book
    {
        Id = "62",
        Title = "The Road",
        Author = "Cormac McCarthy",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "63",
        Title = "Blood Meridian",
        Author = "Cormac McCarthy",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-8).ToString("o")
    },
    new Book
    {
        Id = "64",
        Title = "No Country for Old Men",
        Author = "Cormac McCarthy",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "65",
        Title = "The Kite Runner",
        Author = "Khaled Hosseini",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-6).ToString("o")
    },
    new Book
    {
        Id = "66",
        Title = "A Thousand Splendid Suns",
        Author = "Khaled Hosseini",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "67",
        Title = "And the Mountains Echoed",
        Author = "Khaled Hosseini",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-9).ToString("o")
    },
    new Book
    {
        Id = "68",
        Title = "Fahrenheit 451",
        Author = "Ray Bradbury",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    },
    new Book
    {
        Id = "69",
        Title = "The Martian Chronicles",
        Author = "Ray Bradbury",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
    },
    new Book
    {
        Id = "70",
        Title = "Dune",
        Author = "Frank Herbert",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "71",
        Title = "Foundation",
        Author = "Isaac Asimov",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-8).ToString("o")
    },
    new Book
    {
        Id = "72",
        Title = "I, Robot",
        Author = "Isaac Asimov",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "73",
        Title = "Neuromancer",
        Author = "William Gibson",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-6).ToString("o")
    },
    new Book
    {
        Id = "74",
        Title = "Do Androids Dream of Electric Sheep?",
        Author = "Philip K. Dick",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "75",
        Title = "The Hitchhiker's Guide to the Galaxy",
        Author = "Douglas Adams",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-9).ToString("o")
    },
    new Book
    {
        Id = "76",
        Title = "Ender's Game",
        Author = "Orson Scott Card",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    },
    new Book
    {
        Id = "77",
        Title = "The Name of the Wind",
        Author = "Patrick Rothfuss",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
    },
    new Book
    {
        Id = "78",
        Title = "A Game of Thrones",
        Author = "George R.R. Martin",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "79",
        Title = "The Fellowship of the Ring",
        Author = "J.R.R. Tolkien",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-8).ToString("o")
    },
    new Book
    {
        Id = "80",
        Title = "The Two Towers",
        Author = "J.R.R. Tolkien",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "81",
        Title = "The Return of the King",
        Author = "J.R.R. Tolkien",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-6).ToString("o")
    },
    new Book
    {
        Id = "82",
        Title = "The Silmarillion",
        Author = "J.R.R. Tolkien",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "83",
        Title = "Harry Potter and the Philosopher's Stone",
        Author = "J.K. Rowling",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-9).ToString("o")
    },
    new Book
    {
        Id = "84",
        Title = "Harry Potter and the Chamber of Secrets",
        Author = "J.K. Rowling",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    },
    new Book
    {
        Id = "85",
        Title = "Harry Potter and the Prisoner of Azkaban",
        Author = "J.K. Rowling",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
    },
    new Book
    {
        Id = "86",
        Title = "The Hunger Games",
        Author = "Suzanne Collins",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "87",
        Title = "Catching Fire",
        Author = "Suzanne Collins",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-8).ToString("o")
    },
    new Book
    {
        Id = "88",
        Title = "Mockingjay",
        Author = "Suzanne Collins",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "89",
        Title = "The Giver",
        Author = "Lois Lowry",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-6).ToString("o")
    },
    new Book
    {
        Id = "90",
        Title = "The Fault in Our Stars",
        Author = "John Green",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "91",
        Title = "Looking for Alaska",
        Author = "John Green",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-9).ToString("o")
    },
    new Book
    {
        Id = "92",
        Title = "The Da Vinci Code",
        Author = "Dan Brown",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    },
    new Book
    {
        Id = "93",
        Title = "Angels & Demons",
        Author = "Dan Brown",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
    },
    new Book
    {
        Id = "94",
        Title = "The Girl with the Dragon Tattoo",
        Author = "Stieg Larsson",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-4).ToString("o")
    },
    new Book
    {
        Id = "95",
        Title = "Gone Girl",
        Author = "Gillian Flynn",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-8).ToString("o")
    },
    new Book
    {
        Id = "96",
        Title = "The Shining",
        Author = "Stephen King",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-3).ToString("o")
    },
    new Book
    {
        Id = "97",
        Title = "It",
        Author = "Stephen King",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-6).ToString("o")
    },
    new Book
    {
        Id = "98",
        Title = "The Stand",
        Author = "Stephen King",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
    },
    new Book
    {
        Id = "99",
        Title = "Misery",
        Author = "Stephen King",
        Availability = true,
        LastUpdated = DateTime.UtcNow.AddDays(-9).ToString("o")
    },
    new Book
    {
        Id = "100",
        Title = "The Green Mile",
        Author = "Stephen King",
        Availability = false,
        LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
    }
};