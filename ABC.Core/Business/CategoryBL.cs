using ABC.Common.DTO;
using ABC.Core.Contract;
using ABC.Core.Domain;
using ABC.Core.Validator;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Core.Business
{
    class CategoryBL : Business<ABCEntities, Category, Category, ItemValidator, int>, ICategoryBL
    {
        readonly ABCEntities _ctx;

        public CategoryBL(ABCEntities aBCEntities) : base(aBCEntities)
        {
            //_ctx = aBCEntities;
        }

        public Task<Category> DeleteAsync(Category item)
        {
            throw new NotImplementedException();
        }

        public async Task<Category> GetByKeyAsync(int key)
        {
            return MapToDTO(new Category() { Id = 1, Name = "Test"});
        }

        public Task<DataList<Category>> List(Category filter, PageConfig config)
        {
            throw new NotImplementedException();
        }

        public Task<Category> SaveAsync(Category item)
        {
            throw new NotImplementedException();
        }
    }
}
