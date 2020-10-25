using System;
using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public enum Genre
{
    ActionAndAdventure, Art, AlternateHistory, Autobiography,
    Anthology, Biography, ComicBook, Diary, Dictionary, Crime,
    Encyclopedia, Drama, Guide, Fairytale, Fantasy, Journal,
    Fiction, Horror, Memoir, Mystery, Novel, Paranormal, Poetry,
    Science, Romance, Satire, Travel, ScienceFiction, ShortStory, Thriller, Other
}

namespace CommonData.Models
{
    [Serializable]
    public class Book
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
            
        public string Title { get; set; }

        public string Author { get; set; }

        public string Genre { get; set; }

        public int ReleaseYear { get; set; }

        [BsonElement("ISBN")]
        public string Isbn { get; set; }

        public string RentDate { get; set; }

        public string ReturnDate { get; set; }

        public string Renter { get; set; }
    }
}
