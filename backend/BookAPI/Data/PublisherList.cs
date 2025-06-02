using System;
using BookAPI.Models;

namespace BookAPI.Data;

public static class PublisherList
{
    private static List<Publisher> _publishers = new()
    {
        new Publisher
        {
            Id = 1,
            Name = "Penguin Random House",
            Location = "New York, USA"
        },
        new Publisher
        {
            Id = 2,
            Name = "HarperCollins Publishers",
            Location = "New York, USA"
        },
        new Publisher
        {
            Id = 3,
            Name = "Macmillan Publishers",
            Location = "London, UK"
        },
        new Publisher
        {
            Id = 4,
            Name = "Simon & Schuster",
            Location = "New York, USA"
        },
        new Publisher
        {
            Id = 5,
            Name = "Hachette Book Group",
            Location = "New York, USA"
        },
        new Publisher
        {
            Id = 6,
            Name = "Oxford University Press",
            Location = "Oxford, UK"
        },
        new Publisher
        {
            Id = 7,
            Name = "Scholastic Corporation",
            Location = "New York, USA"
        },
        new Publisher
        {
            Id = 8,
            Name = "Pearson Education",
            Location = "London, UK"
        },
        new Publisher
        {
            Id = 9,
            Name = "Wiley",
            Location = "Hoboken, USA"
        },
        new Publisher
        {
            Id = 10,
            Name = "Springer Nature",
            Location = "Berlin, Germany"
        }
    };

    public static List<Publisher> GetPublishers()
    {
        return _publishers;
    }

    public static Publisher? GetPublisherById(int id)
    {
        return _publishers.FirstOrDefault(p => p.Id == id);
    }

    public static void AddPublisher(Publisher publisher)
    {
        if (publisher != null && !_publishers.Any(p => p.Id == publisher.Id))
        {
            _publishers.Add(publisher);
        }
    }

    public static void RemovePublisher(int id)
    {
        var publisher = GetPublisherById(id);
        if (publisher != null)
        {
            _publishers.Remove(publisher);
        }
    }
    public static void UpdatePublisher(Publisher publisher)
    {
        var existingPublisher = GetPublisherById(publisher.Id);
        if (existingPublisher != null)
        {
            existingPublisher.Name = publisher.Name;
            existingPublisher.Location = publisher.Location;
        }
    }

}
