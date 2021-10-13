using Entity.Enums;
using System;

namespace Entity.Abstract
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        private Status _status = Status.Added;
        public Status Status { get => _status; set => _status = value; }
        private DateTime _createdDate = DateTime.Now;
        public DateTime CreatedDate { get => _createdDate; set => _createdDate = value; }
        public DateTime? ModifiedDate { get; set; }
    }
}
