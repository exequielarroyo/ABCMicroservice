using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;
using LinqKit;
using System.Data.Entity.Infrastructure;
using FluentValidation;
using System.Globalization;

namespace ABC
{
    public abstract class Business<TEntities, TModel, TEntity, TValidator, TId> : BaseBL<TModel, TEntity, TId>, IEntity<TModel, TId> where TEntity : class
    {
        protected internal readonly TEntities _ctx;
        protected readonly int _maxPageSize;

        public Business(TEntities entities)
        {
            _ctx = entities;
        }

        public static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string[] parts = input.Split('_', ' ');
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = textInfo.ToTitleCase(parts[i].ToLower());
            }
            return string.Concat(parts);
        }

        #region BaseBL
        protected override TEntity MapToEntity(TModel item)
        {
            if (item == null) return Activator.CreateInstance<TEntity>();

            var entity = Activator.CreateInstance<TEntity>();

            foreach (var property in typeof(TEntity).GetProperties()) {
                var modelProperty = typeof(TModel).GetProperty(ToPascalCase(property.Name));

                if (modelProperty != null)
                {
                    var value = modelProperty.GetValue(item);
                    property.SetValue(entity, value);
                }
            }

            return entity;
        }

        protected override IQueryable<TEntity> FilteredEntities(TModel filter, IQueryable<TEntity> custom_query = null, bool strict = false)
        {
            //var predicate = PredicateBuilder.New<TEntity>(true);
            //if (filter.Id != null && filter.Id != 0)
            //    predicate = predicate.And(x => x.ID == filter.Id);
            //if (!string.IsNullOrEmpty(filter.Name)) predicate = (strict)
            //                ? predicate.And(x => x.NAME.ToLower() == filter.Name.ToLower())
            //                : predicate.And(x => x.NAME.ToLower().Contains(filter.Name.ToLower()));

            //var query = custom_query ?? _ctx.ITEMS;
            //return query.Where(predicate);

            var predicate = PredicateBuilder.New<TEntity>(true);
            var idProperty = typeof(TModel).GetProperty("Id");
            var IDProperty = typeof(TEntity).GetProperty("ID");

            if (idProperty.GetValue(filter) != null)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");
                var propertyAccess = Expression.PropertyOrField(parameter, idProperty.Name);
                var equals = Expression.Equal(propertyAccess, Expression.Constant(idProperty.GetValue(filter)));

                var and = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

                predicate = predicate.And(and);
            }

            var dbSetProperty = typeof(TEntities).GetProperty("ITEMS");
            
            var query = custom_query ?? dbSetProperty.GetValue(_ctx) as IQueryable<TEntity>;

            return query.Where(predicate);

            //throw new NotImplementedException();
        }

        protected override TId GenerateId(string sequenceName = null)
        {
            sequenceName = "PKSEQ_ITEMS";
            var databaseProperty = typeof(TEntities).GetProperty("Database");
            var database = databaseProperty.GetValue(_ctx) as Database;
            
            return database.SqlQuery<TId>(string.Format("Select {0}.nextval from DUAL", sequenceName)).FirstOrDefault(); ;
        }

        protected async override Task<TModel> Insert(TModel item)
        {
            var idProperty = typeof(TModel).GetProperty("Id");
            idProperty.SetValue(item, GenerateId());
            TEntity data = MapToEntity(item);

            var x = _ctx as DbContext;
            x.Entry(data).State = EntityState.Added;

            if (await x.SaveChangesAsync() <= 0)
            {
                var validationProperty = typeof(TModel).GetProperty("Validation");
                validationProperty.SetValue(item, CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBSaveError, "Item"));
            }
            return item;
        }

        protected override TModel MapToDTO(TEntity item)
        {
            if (item == null) return Activator.CreateInstance<TModel>();

            var model = Activator.CreateInstance<TModel>();

            foreach (var property in typeof(TModel).GetProperties())
            {
                var modelProperty = typeof(TEntity).GetProperty(property.Name.ToUpper());

                if (modelProperty != null)
                {
                    var value = modelProperty.GetValue(item);
                    property.SetValue(model, value);
                }
            }

            return model;
        }

        protected override IQueryable<TEntity> OrderEntities(IQueryable<TEntity> query, string sortOrder, bool isAscending)
        {
            //switch (sortOrder.ToUpper())
            //{
            //    case "ID":
            //        query = isAscending ? query.OrderBy(x => x.ID) : query.OrderByDescending(x => x.ID);
            //        break;
            //    case "NAME":
            //        query = isAscending ? query.OrderBy(x => x.NAME) : query.OrderByDescending(x => x.NAME);
            //        break;
            //    case "DESCRIPTION":
            //        query = isAscending ? query.OrderBy(x => x.DESCRIPTION) : query.OrderByDescending(x => x.DESCRIPTION);
            //        break;
            //    default:
            //        query = query.OrderBy(x => x.ID);
            //        break;
            //}
            //return query;

            return query;
        }

        protected override IQueryable<TModel> QueryToDTO(IQueryable<TEntity> query)
        {
            //return query.Select(x => new Item
            //{
            //    Id = x.ID,
            //    Name = x.NAME,
            //    Description = x.DESCRIPTION,
            //    TransContext = new TransactionContext(),
            //    Validation = new ValidationResult()
            //});
            var IDProperty = typeof(TEntity).GetProperty("ID");

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var propertyAccess = Expression.PropertyOrField(parameter, IDProperty.Name);

            var modelType = typeof(TModel);
            var entityType = typeof(TEntity);
            var modelConstructor = modelType.GetConstructor(Type.EmptyTypes);
            var modelInstance = Expression.MemberInit(Expression.New(modelConstructor), Expression.Bind(modelType.GetProperty("Id"), Expression.PropertyOrField(parameter, "ID")));
            var selectLambda = Expression.Lambda<Func<TEntity, TModel>>(modelInstance, parameter);

            //var select = Expression.Lambda<Func<TEntity, TModel>>(entityParameter, entityParameter);

            //return query.Select(x => Activator.CreateInstance<TModel>());
            return query.Select(selectLambda);
        }

        protected override Task<TModel> Update(TModel item)
        {
            throw new NotImplementedException();
        }

        #endregion


        public async virtual Task<TModel> GetByKeyAsync(TId key)
        {
            var dbSetProperty = typeof(TEntities).GetProperty("ITEMS");

            var dbSet = dbSetProperty.GetValue(_ctx) as DbSet<TEntity>;

            var idProperty = typeof(TEntity).GetProperties().FirstOrDefault(p => p.Name.ToUpper() == "ID");

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var propertyAccess = Expression.PropertyOrField(parameter, idProperty.Name);
            var equals = Expression.Equal(propertyAccess, Expression.Constant(key));

            var predicate = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

            var item = await dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);

            return MapToDTO(item);
        }

        public async Task<DataList<TModel>> List(TModel filter, PageConfig config)
        {
            IQueryable<TEntity> query = FilteredEntities(filter);

            string resolved_sort = config.SortBy ?? "Id";
            bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

            int resolved_size = config.Size ?? _maxPageSize;
            if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
            int resolved_index = config.Index ?? 1;

            query = OrderEntities(query, resolved_sort, resolved_isAscending);
            var items = query.ToListAsync();
            var paged = PagedQuery(query, resolved_size, resolved_index);
            items = paged.ToListAsync();
            return new DataList<TModel>
            {
                Count = await query.CountAsync(),
                Items = await QueryToDTO(paged).ToListAsync()
            };
        }

        public async Task<TModel> SaveAsync(TModel item)
        {
            //item.Validation = new ItemValidator(_ctx).Validate(item);


            var validatorProperty = typeof(TModel).GetProperty("Validation");
            //var validator = validatorProperty.GetValue(item) as AbstractValidator<TModel>;
            var validator = Activator.CreateInstance(typeof(TValidator), _ctx as DbContext) as AbstractValidator<TModel>;

            validatorProperty.SetValue(item, validator.Validate(item));
            var x = item as BaseDTO;

            if (!x.Validation.IsValid) return item;

            var idProperty = typeof(TModel).GetProperty("Id");
            return (Convert.ToInt32(idProperty.GetValue(item)) != 0) ? await Update(item) : await Insert(item);
        }

        public Task<TModel> DeleteAsync(TModel item)
        {
            throw new NotImplementedException();
        }
    }
}
