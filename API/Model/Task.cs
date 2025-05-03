using Microsoft.AspNetCore.Http.HttpResults;
using System;

namespace API.Model
{
    public class Task
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Deadline Deadline { get; set; }
        public string Id { get; set; }
        public string UserId { get; set; }
        public Status Status { get; set; } = Status.Incompleted;


        public Task(string name, string description, Deadline deadline, string userId)
        {
            Name = name;
            Description = description;
            Deadline = deadline;
            UserId = userId;
            Id = Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return $"Nama: {Name}, Deskripsi: {Description}, Deadline: {Deadline.Day}/{Deadline.Month}/{Deadline.Year} {Deadline.Hour:D2}:{Deadline.Minute:D2}, Status: {Status}, Dibuat oleh: {UserId}";
        }

    }
}
