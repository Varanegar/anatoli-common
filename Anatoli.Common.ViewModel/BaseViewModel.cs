using System;

namespace Anatoli.Common.ViewModel
{
    public class CBaseViewModel
    {
        public virtual int ID { get; set; }
        public virtual Guid UniqueId { get; set; }
        public virtual Guid ApplicationOwnerId { get; set; }
        public virtual Guid DataOwnerId { get; set; }
        public virtual Guid DataOwnerCenterId { get; set; }
        public virtual bool IsRemoved { get; set; }
        public virtual DateTime CreatedDate { get; set; }
        public virtual DateTime LastUpdate { get; set; }
    }
}
