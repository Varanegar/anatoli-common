﻿using Anatoli.DataAccess.Models.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anatoli.Common.DataAccess.Models
{
    public abstract class BaseModel
    {
        [Key]
        public Guid Id { get; set; }
        public int Number_ID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool IsRemoved { get; set; }

        [ForeignKey("ApplicationOwner")]
        public virtual Guid ApplicationOwnerId { get; set; }
        [ForeignKey("DataOwner")]
        public virtual Guid DataOwnerId { get; set; }
        [ForeignKey("DataOwnerCenter")]
        public virtual Guid DataOwnerCenterId { get; set; }

        public virtual DataOwnerCenter DataOwnerCenter { get; set; }
        public virtual DataOwner DataOwner { get; set; }
        public virtual ApplicationOwner ApplicationOwner { get; set; }
        public virtual Principal AddedBy { get; set; }
        public virtual Principal LastModifiedBy { get; set; }

        [ForeignKey("AddedBy")]
        public virtual Guid? AddedById { get; set; }
        [ForeignKey("LastModifiedBy")]
        public virtual Guid? LastModifiedById { get; set; }
    }
}
