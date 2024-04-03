using ABC.Common.DTO;
using ABC.Core.Contract;
using ABC.Core.Domain;
using ABC.Core.Validator;
using FluentValidation.Results;
using LinqKit;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Core.Business
{
    internal class ItemsBLL : BaseBL<Item, ITEM, decimal>, IItemsBL
    {
        readonly ABCEntities _ctx;
        readonly int _maxPageSize;
        public ItemsBLL(ABCEntities aBCEntities)
        {
            _ctx = aBCEntities;
        }

        public async Task<Item> DeleteAsync(Item item)
        {
            ITEM data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Deleted;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBDeleteError, "Item");
            if (item.Validation == null) item.Validation = new ValidationResult();
            return item;
        }

        public async Task<Item> GetByKeyAsync(decimal key)
        {
            ITEM item = await _ctx.ITEMS.AsNoTracking().FirstOrDefaultAsync(x => x.ID == key);
            return MapToDTO(item);
        }

        public async Task<DataList<Item>> List(Item filter, PageConfig config)
        {
            IQueryable<ITEM> query = FilteredEntities(filter);

            string resolved_sort = config.SortBy ?? "Id";
            bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

            int resolved_size = config.Size ?? _maxPageSize;
            if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
            int resolved_index = config.Index ?? 1;

            query = OrderEntities(query, resolved_sort, resolved_isAscending);
            var paged = PagedQuery(query, resolved_size, resolved_index);
            return new DataList<Item>
            {
                Count = await query.CountAsync(),
                Items = await QueryToDTO(paged).ToListAsync()
            };
        }

        public async Task<Item> SaveAsync(Item item)
        {
            item.Validation = new ItemValidator(_ctx).Validate(item);
            if (!item.Validation.IsValid) return item;
            return (item.Id != 0) ? await Update(item) : await Insert(item);
        }

        protected override IQueryable<ITEM> FilteredEntities(Item filter, IQueryable<ITEM> custom_query = null, bool strict = false)
        {
            var predicate = PredicateBuilder.New<ITEM>(true);
            if (filter.Id != null && filter.Id != 0)
                predicate = predicate.And(x => x.ID == filter.Id);
            if (!string.IsNullOrEmpty(filter.Name)) predicate = (strict)
                            ? predicate.And(x => x.NAME.ToLower() == filter.Name.ToLower())
                            : predicate.And(x => x.NAME.ToLower().Contains(filter.Name.ToLower()));

            var query = custom_query ?? _ctx.ITEMS;
            return query.Where(predicate);
        }

        protected override decimal GenerateId(string sequenceName = null)
        {
            sequenceName = "PKSEQ_ITEMS";
            return _ctx.Database.SqlQuery<int>(string.Format("Select {0}.nextval from DUAL", sequenceName)).FirstOrDefault();
        }

        protected async override Task<Item> Insert(Item item)
        {
            item.Id = GenerateId();
            ITEM data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Added;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBSaveError, "Item");
            return item;
        }

        protected override Item MapToDTO(ITEM item)
        {
            if (item == null) return new Item();
            return new Item
            {
                Id = item.ID,
                Name = item.NAME,
                Description = item.DESCRIPTION,
                TransContext = new TransactionContext(),
                Validation = new ValidationResult()
            };
        }

        protected override ITEM MapToEntity(Item item)
        {
            if (item == null) return new ITEM();
            return new ITEM
            {
                ID = item.Id,
                NAME = item.Name,
                DESCRIPTION = item.Description
            };
        }

        protected override IQueryable<ITEM> OrderEntities(IQueryable<ITEM> query, string sortOrder, bool isAscending)
        {
            switch (sortOrder.ToUpper())
            {
                case "ID":
                    query = isAscending ? query.OrderBy(x => x.ID) : query.OrderByDescending(x => x.ID);
                    break;
                case "NAME":
                    query = isAscending ? query.OrderBy(x => x.NAME) : query.OrderByDescending(x => x.NAME);
                    break;
                case "DESCRIPTION":
                    query = isAscending ? query.OrderBy(x => x.DESCRIPTION) : query.OrderByDescending(x => x.DESCRIPTION);
                    break;
                default:
                    query = query.OrderBy(x => x.ID);
                    break;
            }
            return query;
        }

        protected override IQueryable<Item> QueryToDTO(IQueryable<ITEM> query)
        {
            return query.Select(x => new Item
            {
                Id = x.ID,
                Name = x.NAME,
                Description = x.DESCRIPTION,
                TransContext = new TransactionContext(),
                Validation = new ValidationResult()
            });
        }

        protected async override Task<Item> Update(Item item)
        {
            ITEM data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Modified;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBSaveError, "Item");
            return item;
        }
    }
}
