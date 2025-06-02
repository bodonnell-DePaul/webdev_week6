using System;
using BookAPI.Models;

namespace BookAPI.Data;

public static class BookList
{
    public static List<Book> _books = new()
    {
        new Book
        {
            Id = 1,
            Title = "The Great Gatsby",
            Author = "F. Scott Fitzgerald",
            Year = 1925,
            Genre = "Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 3
        },
        new Book
        {
            Id = 2,
            Title = "To Kill a Mockingbird",
            Author = "Harper Lee",
            Year = 1960,
            Genre = "Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 7
        },
        new Book
        {
            Id = 3,
            Title = "1984",
            Author = "George Orwell",
            Year = 1949,
            Genre = "Dystopian Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 5
        },
        new Book
        {
            Id = 4,
            Title = "Pride and Prejudice",
            Author = "Jane Austen",
            Year = 1813,
            Genre = "Romance",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 2
        },
        new Book
        {
            Id = 5,
            Title = "The Catcher in the Rye",
            Author = "J.D. Salinger",
            Year = 1951,
            Genre = "Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 8
        },
        new Book
        {
            Id = 6,
            Title = "Brave New World",
            Author = "Aldous Huxley",
            Year = 1932,
            Genre = "Science Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 4
        },
        new Book
        {
            Id = 7,
            Title = "The Lord of the Rings",
            Author = "J.R.R. Tolkien",
            Year = 1954,
            Genre = "Fantasy",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 1
        },
        new Book
        {
            Id = 8,
            Title = "Animal Farm",
            Author = "George Orwell",
            Year = 1945,
            Genre = "Political Satire",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 5
        },
        new Book
        {
            Id = 9,
            Title = "The Hobbit",
            Author = "J.R.R. Tolkien",
            Year = 1937,
            Genre = "Fantasy",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 1
        },
        new Book
        {
            Id = 10,
            Title = "Moby-Dick",
            Author = "Herman Melville",
            Year = 1851,
            Genre = "Adventure",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 9
        },
        new Book
        {
            Id = 11,
            Title = "Wuthering Heights",
            Author = "Emily Brontë",
            Year = 1847,
            Genre = "Gothic Fiction",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 6
        },
        new Book
        {
            Id = 12,
            Title = "The Odyssey",
            Author = "Homer",
            Year = -800,
            Genre = "Epic Poetry",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 10
        },
        new Book
        {
            Id = 13,
            Title = "Jane Eyre",
            Author = "Charlotte Brontë",
            Year = 1847,
            Genre = "Gothic Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 6
        },
        new Book
        {
            Id = 14,
            Title = "Frankenstein",
            Author = "Mary Shelley",
            Year = 1818,
            Genre = "Gothic Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 3
        },
        new Book
        {
            Id = 15,
            Title = "The Picture of Dorian Gray",
            Author = "Oscar Wilde",
            Year = 1890,
            Genre = "Gothic Fiction",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 7
        },
        new Book
        {
            Id = 16,
            Title = "Crime and Punishment",
            Author = "Fyodor Dostoevsky",
            Year = 1866,
            Genre = "Psychological Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 4
        },
        new Book
        {
            Id = 17,
            Title = "The Brothers Karamazov",
            Author = "Fyodor Dostoevsky",
            Year = 1880,
            Genre = "Philosophical Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 4
        },
        new Book
        {
            Id = 18,
            Title = "War and Peace",
            Author = "Leo Tolstoy",
            Year = 1869,
            Genre = "Historical Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 2
        },
        new Book
        {
            Id = 19,
            Title = "Anna Karenina",
            Author = "Leo Tolstoy",
            Year = 1878,
            Genre = "Realist Fiction",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 2
        },
        new Book
        {
            Id = 20,
            Title = "Don Quixote",
            Author = "Miguel de Cervantes",
            Year = 1605,
            Genre = "Adventure",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 8
        },
        new Book
        {
            Id = 21,
            Title = "The Divine Comedy",
            Author = "Dante Alighieri",
            Year = 1320,
            Genre = "Epic Poetry",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 10
        },
        new Book
        {
            Id = 22,
            Title = "Les Misérables",
            Author = "Victor Hugo",
            Year = 1862,
            Genre = "Historical Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 9
        },
        new Book
        {
            Id = 23,
            Title = "The Grapes of Wrath",
            Author = "John Steinbeck",
            Year = 1939,
            Genre = "Social Realism",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 1
        },
        new Book
        {
            Id = 24,
            Title = "East of Eden",
            Author = "John Steinbeck",
            Year = 1952,
            Genre = "Fiction",
            IsAvailable = false,
            AudioBookAvailable = false,
            PublisherId = 1
        },
        new Book
        {
            Id = 25,
            Title = "One Hundred Years of Solitude",
            Author = "Gabriel García Márquez",
            Year = 1967,
            Genre = "Magical Realism",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 5
        },
        new Book
        {
            Id = 26,
            Title = "Love in the Time of Cholera",
            Author = "Gabriel García Márquez",
            Year = 1985,
            Genre = "Romance",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 5
        },
        new Book
        {
            Id = 27,
            Title = "The Old Man and the Sea",
            Author = "Ernest Hemingway",
            Year = 1952,
            Genre = "Fiction",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 3
        },
        new Book
        {
            Id = 28,
            Title = "For Whom the Bell Tolls",
            Author = "Ernest Hemingway",
            Year = 1940,
            Genre = "War Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 3
        },
        new Book
        {
            Id = 29,
            Title = "A Farewell to Arms",
            Author = "Ernest Hemingway",
            Year = 1929,
            Genre = "War Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 3
        },
        new Book
        {
            Id = 30,
            Title = "The Sun Also Rises",
            Author = "Ernest Hemingway",
            Year = 1926,
            Genre = "Fiction",
            IsAvailable = false,
            AudioBookAvailable = false,
            PublisherId = 3
        },
        new Book
        {
            Id = 31,
            Title = "The Count of Monte Cristo",
            Author = "Alexandre Dumas",
            Year = 1844,
            Genre = "Adventure",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 7
        },
        new Book
        {
            Id = 32,
            Title = "The Three Musketeers",
            Author = "Alexandre Dumas",
            Year = 1844,
            Genre = "Adventure",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 7
        },
        new Book
        {
            Id = 33,
            Title = "Ulysses",
            Author = "James Joyce",
            Year = 1922,
            Genre = "Modernist Fiction",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 8
        },
        new Book
        {
            Id = 34,
            Title = "Dubliners",
            Author = "James Joyce",
            Year = 1914,
            Genre = "Short Stories",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 8
        },
        new Book
        {
            Id = 35,
            Title = "Great Expectations",
            Author = "Charles Dickens",
            Year = 1861,
            Genre = "Coming-of-age",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 6
        },
        new Book
        {
            Id = 36,
            Title = "David Copperfield",
            Author = "Charles Dickens",
            Year = 1850,
            Genre = "Fiction",
            IsAvailable = false,
            AudioBookAvailable = false,
            PublisherId = 6
        },
        new Book
        {
            Id = 37,
            Title = "A Tale of Two Cities",
            Author = "Charles Dickens",
            Year = 1859,
            Genre = "Historical Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 6
        },
        new Book
        {
            Id = 38,
            Title = "Oliver Twist",
            Author = "Charles Dickens",
            Year = 1838,
            Genre = "Social Criticism",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 6
        },
        new Book
        {
            Id = 39,
            Title = "Bleak House",
            Author = "Charles Dickens",
            Year = 1853,
            Genre = "Social Criticism",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 6
        },
        new Book
        {
            Id = 40,
            Title = "The Adventures of Huckleberry Finn",
            Author = "Mark Twain",
            Year = 1884,
            Genre = "Adventure",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 4
        },
        new Book
        {
            Id = 41,
            Title = "The Adventures of Tom Sawyer",
            Author = "Mark Twain",
            Year = 1876,
            Genre = "Adventure",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 4
        },
        new Book
        {
            Id = 42,
            Title = "Catch-22",
            Author = "Joseph Heller",
            Year = 1961,
            Genre = "Dark Comedy",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 9
        },
        new Book
        {
            Id = 43,
            Title = "Slaughterhouse-Five",
            Author = "Kurt Vonnegut",
            Year = 1969,
            Genre = "Science Fiction",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 2
        },
        new Book
        {
            Id = 44,
            Title = "The Handmaid's Tale",
            Author = "Margaret Atwood",
            Year = 1985,
            Genre = "Dystopian Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 10
        },
        new Book
        {
            Id = 45,
            Title = "The Bell Jar",
            Author = "Sylvia Plath",
            Year = 1963,
            Genre = "Semi-autobiographical",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 1
        },
        new Book
        {
            Id = 46,
            Title = "Mrs. Dalloway",
            Author = "Virginia Woolf",
            Year = 1925,
            Genre = "Modernist Fiction",
            IsAvailable = false,
            AudioBookAvailable = false,
            PublisherId = 8
        },
        new Book
        {
            Id = 47,
            Title = "To the Lighthouse",
            Author = "Virginia Woolf",
            Year = 1927,
            Genre = "Modernist Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 8
        },
        new Book
        {
            Id = 48,
            Title = "The Sound and the Fury",
            Author = "William Faulkner",
            Year = 1929,
            Genre = "Southern Gothic",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 5
        },
        new Book
        {
            Id = 49,
            Title = "As I Lay Dying",
            Author = "William Faulkner",
            Year = 1930,
            Genre = "Southern Gothic",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 5
        },
        new Book
        {
            Id = 50,
            Title = "Heart of Darkness",
            Author = "Joseph Conrad",
            Year = 1899,
            Genre = "Novella",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 7
        },
        new Book
        {
            Id = 51,
            Title = "Lord of the Flies",
            Author = "William Golding",
            Year = 1954,
            Genre = "Allegorical Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 9
        },
        new Book
        {
            Id = 52,
            Title = "The Alchemist",
            Author = "Paulo Coelho",
            Year = 1988,
            Genre = "Philosophical Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 3
        },
        new Book
        {
            Id = 53,
            Title = "The Little Prince",
            Author = "Antoine de Saint-Exupéry",
            Year = 1943,
            Genre = "Children's Literature",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 4
        },
        new Book
        {
            Id = 54,
            Title = "The Stranger",
            Author = "Albert Camus",
            Year = 1942,
            Genre = "Philosophical Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 2
        },
        new Book
        {
            Id = 55,
            Title = "The Plague",
            Author = "Albert Camus",
            Year = 1947,
            Genre = "Philosophical Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 2
        },
        new Book
        {
            Id = 56,
            Title = "Invisible Man",
            Author = "Ralph Ellison",
            Year = 1952,
            Genre = "Social Commentary",
            IsAvailable = false,
            AudioBookAvailable = false,
            PublisherId = 1
        },
        new Book
        {
            Id = 57,
            Title = "Native Son",
            Author = "Richard Wright",
            Year = 1940,
            Genre = "Social Protest",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 6
        },
        new Book
        {
            Id = 58,
            Title = "Beloved",
            Author = "Toni Morrison",
            Year = 1987,
            Genre = "Historical Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 10
        },
        new Book
        {
            Id = 59,
            Title = "Song of Solomon",
            Author = "Toni Morrison",
            Year = 1977,
            Genre = "Magical Realism",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 10
        },
        new Book
        {
            Id = 60,
            Title = "Things Fall Apart",
            Author = "Chinua Achebe",
            Year = 1958,
            Genre = "Postcolonial Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 8
        },
        new Book
        {
            Id = 61,
            Title = "The Color Purple",
            Author = "Alice Walker",
            Year = 1982,
            Genre = "Epistolary Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 7
        },
        new Book
        {
            Id = 62,
            Title = "The Road",
            Author = "Cormac McCarthy",
            Year = 2006,
            Genre = "Post-apocalyptic Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 9
        },
        new Book
        {
            Id = 63,
            Title = "Blood Meridian",
            Author = "Cormac McCarthy",
            Year = 1985,
            Genre = "Western",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 9
        },
        new Book
        {
            Id = 64,
            Title = "No Country for Old Men",
            Author = "Cormac McCarthy",
            Year = 2005,
            Genre = "Neo-Western",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 9
        },
        new Book
        {
            Id = 65,
            Title = "The Kite Runner",
            Author = "Khaled Hosseini",
            Year = 2003,
            Genre = "Historical Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 4
        },
        new Book
        {
            Id = 66,
            Title = "A Thousand Splendid Suns",
            Author = "Khaled Hosseini",
            Year = 2007,
            Genre = "Historical Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 4
        },
        new Book
        {
            Id = 67,
            Title = "And the Mountains Echoed",
            Author = "Khaled Hosseini",
            Year = 2013,
            Genre = "Family Saga",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 4
        },
        new Book
        {
            Id = 68,
            Title = "Fahrenheit 451",
            Author = "Ray Bradbury",
            Year = 1953,
            Genre = "Dystopian Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 5
        },
        new Book
        {
            Id = 69,
            Title = "The Martian Chronicles",
            Author = "Ray Bradbury",
            Year = 1950,
            Genre = "Science Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 5
        },
        new Book
        {
            Id = 70,
            Title = "Dune",
            Author = "Frank Herbert",
            Year = 1965,
            Genre = "Science Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 1
        },
        new Book
        {
            Id = 71,
            Title = "Foundation",
            Author = "Isaac Asimov",
            Year = 1951,
            Genre = "Science Fiction",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 3
        },
        new Book
        {
            Id = 72,
            Title = "I, Robot",
            Author = "Isaac Asimov",
            Year = 1950,
            Genre = "Science Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 3
        },
        new Book
        {
            Id = 73,
            Title = "Neuromancer",
            Author = "William Gibson",
            Year = 1984,
            Genre = "Cyberpunk",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 2
        },
        new Book
        {
            Id = 74,
            Title = "Do Androids Dream of Electric Sheep?",
            Author = "Philip K. Dick",
            Year = 1968,
            Genre = "Science Fiction",
            IsAvailable = false,
            AudioBookAvailable = false,
            PublisherId = 8
        },
        new Book
        {
            Id = 75,
            Title = "The Hitchhiker's Guide to the Galaxy",
            Author = "Douglas Adams",
            Year = 1979,
            Genre = "Science Fiction Comedy",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 6
        },
        new Book
        {
            Id = 76,
            Title = "Ender's Game",
            Author = "Orson Scott Card",
            Year = 1985,
            Genre = "Science Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 7
        },
        new Book
        {
            Id = 77,
            Title = "The Name of the Wind",
            Author = "Patrick Rothfuss",
            Year = 2007,
            Genre = "Fantasy",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 10
        },
        new Book
        {
            Id = 78,
            Title = "A Game of Thrones",
            Author = "George R.R. Martin",
            Year = 1996,
            Genre = "Epic Fantasy",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 1
        },
        new Book
        {
            Id = 79,
            Title = "The Fellowship of the Ring",
            Author = "J.R.R. Tolkien",
            Year = 1954,
            Genre = "Fantasy",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 1
        },
        new Book
        {
            Id = 80,
            Title = "The Two Towers",
            Author = "J.R.R. Tolkien",
            Year = 1954,
            Genre = "Fantasy",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 1
        },
        new Book
        {
            Id = 81,
            Title = "The Return of the King",
            Author = "J.R.R. Tolkien",
            Year = 1955,
            Genre = "Fantasy",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 1
        },
        new Book
        {
            Id = 82,
            Title = "The Silmarillion",
            Author = "J.R.R. Tolkien",
            Year = 1977,
            Genre = "Fantasy",
            IsAvailable = false,
            AudioBookAvailable = false,
            PublisherId = 1
        },
        new Book
        {
            Id = 83,
            Title = "Harry Potter and the Philosopher's Stone",
            Author = "J.K. Rowling",
            Year = 1997,
            Genre = "Fantasy",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 9
        },
        new Book
        {
            Id = 84,
            Title = "Harry Potter and the Chamber of Secrets",
            Author = "J.K. Rowling",
            Year = 1998,
            Genre = "Fantasy",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 9
        },
        new Book
        {
            Id = 85,
            Title = "Harry Potter and the Prisoner of Azkaban",
            Author = "J.K. Rowling",
            Year = 1999,
            Genre = "Fantasy",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 9
        },
        new Book
        {
            Id = 86,
            Title = "The Hunger Games",
            Author = "Suzanne Collins",
            Year = 2008,
            Genre = "Dystopian Fiction",
            IsAvailable = false,
            AudioBookAvailable = false,
            PublisherId = 2
        },
        new Book
        {
            Id = 87,
            Title = "Catching Fire",
            Author = "Suzanne Collins",
            Year = 2009,
            Genre = "Dystopian Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 2
        },
        new Book
        {
            Id = 88,
            Title = "Mockingjay",
            Author = "Suzanne Collins",
            Year = 2010,
            Genre = "Dystopian Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 2
        },
        new Book
        {
            Id = 89,
            Title = "The Giver",
            Author = "Lois Lowry",
            Year = 1993,
            Genre = "Dystopian Fiction",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 5
        },
        new Book
        {
            Id = 90,
            Title = "The Fault in Our Stars",
            Author = "John Green",
            Year = 2012,
            Genre = "Young Adult Fiction",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 8
        },
        new Book
        {
            Id = 91,
            Title = "Looking for Alaska",
            Author = "John Green",
            Year = 2005,
            Genre = "Young Adult Fiction",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 8
        },
        new Book
        {
            Id = 92,
            Title = "The Da Vinci Code",
            Author = "Dan Brown",
            Year = 2003,
            Genre = "Thriller",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 3
        },
        new Book
        {
            Id = 93,
            Title = "Angels & Demons",
            Author = "Dan Brown",
            Year = 2000,
            Genre = "Thriller",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 3
        },
        new Book
        {
            Id = 94,
            Title = "The Girl with the Dragon Tattoo",
            Author = "Stieg Larsson",
            Year = 2005,
            Genre = "Crime Thriller",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 7
        },
        new Book
        {
            Id = 95,
            Title = "Gone Girl",
            Author = "Gillian Flynn",
            Year = 2012,
            Genre = "Psychological Thriller",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 10
        },
        new Book
        {
            Id = 96,
            Title = "The Shining",
            Author = "Stephen King",
            Year = 1977,
            Genre = "Horror",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 4
        },
        new Book
        {
            Id = 97,
            Title = "It",
            Author = "Stephen King",
            Year = 1986,
            Genre = "Horror",
            IsAvailable = true,
            AudioBookAvailable = true,
            PublisherId = 4
        },
        new Book
        {
            Id = 98,
            Title = "The Stand",
            Author = "Stephen King",
            Year = 1978,
            Genre = "Post-apocalyptic Horror",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 4
        },
        new Book
        {
            Id = 99,
            Title = "Misery",
            Author = "Stephen King",
            Year = 1987,
            Genre = "Psychological Horror",
            IsAvailable = true,
            AudioBookAvailable = false,
            PublisherId = 4
        },
        new Book
        {
            Id = 100,
            Title = "The Green Mile",
            Author = "Stephen King",
            Year = 1996,
            Genre = "Fantasy Drama",
            IsAvailable = false,
            AudioBookAvailable = true,
            PublisherId = 4
        }
    };

    public static List<Book> GetBooks()
    {
        return _books;
    }

    public static Book? GetBookById(int id)
    {
        return _books.FirstOrDefault(p => p.Id == id);
    }

    public static void AddBook(Book book)
    {
        if (book != null && !_books.Any(p => p.Id == book.Id))
        {
            _books.Add(book);
        }
    }

    public static void RemovePublisher(int id)
    {
        var book = GetBookById(id);
        if (book != null)
        {
            _books.Remove(book);
        }
    }
    public static void UpdateBook(Book book)
    {
        var existingBook = GetBookById(book.Id);
        if (existingBook != null)
        {
            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.Year = book.Year;
            existingBook.Genre = book.Genre;
            existingBook.IsAvailable = book.IsAvailable;
            existingBook.AudioBookAvailable = book.AudioBookAvailable;
            existingBook.PublisherId = book.PublisherId;
            existingBook.Publisher = book.Publisher; // Assuming Publisher is a reference type
        }
    }
    
}