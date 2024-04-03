using ABC.Common.DTO;
using ABC.Core.Contract;
using ABC.Core.Domain;
using ABC.Core.Validator;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Core.Business
{
    class ItemsBL : Business<ABCEntities, Item, ITEM, ItemValidator, decimal>, IItemsBL
    {
        public ItemsBL(ABCEntities entities) : base(entities)
        {
        }

        public override Task<Item> GetByKeyAsync(decimal key)
        {
            return base.GetByKeyAsync(key);
        }

        //public async override Task<Item> GetByKeyAsync(decimal key)
        //{
        //    ITEM item = await _ctx.ITEMS.AsNoTracking().FirstOrDefaultAsync(x => x.ID == key);
        //    return MapToDTO(item);
        //}
    }
}
