using ABC.Common.DTO;
using ABC.Core.Domain;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Core.Validator
{
    internal class ItemValidator : AbstractValidator<Item>
    {
        readonly ABCEntities _ctx;
        public ItemValidator(ABCEntities aBCEntities)
        {
            _ctx = aBCEntities;
        }
    }
}
