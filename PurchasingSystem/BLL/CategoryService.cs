using PurchasingSystem.DAL;
using PurchasingSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasingSystem.BLL
{
    public class CategoryService
    {
        #region Date Field
        private readonly EBikeContext _context;
        #endregion

        // Construction for CategoryService class
        internal CategoryService(EBikeContext context)
        {
            // If the passed in context is null, throw an exception.
            _context = context ?? throw new ArgumentException(nameof(context)); ;
        }
        public List<CategoryView> GetAllCategories()
        {
            return _context.Categories
                .OrderBy(x => x.Description)
                .Select(x => new CategoryView { 
                    CategoryID = x.CategoryID,
                    Description = x.Description,
                    RemoveFromViewFlag = x.RemoveFromViewFlag
                })
                .ToList();
        }
    }
}
