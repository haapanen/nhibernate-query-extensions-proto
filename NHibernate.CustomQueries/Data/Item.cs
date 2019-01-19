using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace NHibernate.CustomQueries.Data
{
    public class Item
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual ICollection<ItemOperation> Operations { get; set; }
    }

    public class ItemOperation
    {
        public virtual int Id { get; set; }
        public virtual int OperationPeriod { get; set; }
        public virtual string OperationPeriodType { get; set; }
        public virtual ICollection<ItemOperationEntry> Entries { get; set; }
    }

    public class ItemOperationEntry
    {
        public virtual int Id { get; set; }
        public virtual string Comment { get; set; }
        public virtual DateTime CreatedAt { get; set; }
    }

    public class ItemMap : ClassMap<Item>
    {
        public ItemMap()
        {
            Table("Item");

            Id(x => x.Id).GeneratedBy.Identity();;
            Map(x => x.Name);

            HasMany(x => x.Operations).Cascade.All();
        }
    }

    public class ItemOperationMap : ClassMap<ItemOperation>
    {
        public ItemOperationMap()
        {
            Table("ItemOperation");

            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.OperationPeriod);
            Map(x => x.OperationPeriodType);
            HasMany(x => x.Entries).Cascade.All();
        }
    }

    public class ItemOperationEntryMap : ClassMap<ItemOperationEntry>
    {
        public ItemOperationEntryMap()
        {
            Table("ItemOperationEntry");

            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.Comment);
            Map(x => x.CreatedAt);
        }
    }
}
