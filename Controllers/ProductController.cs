using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using static Umbraco.Cms.Core.Collections.TopoGraph;
using Umbraco.Cms.Web.Common;
using System.Linq;
using Umbraco.Extensions;
using static CBWEB.Helpers.UmbracoEnums;
using StackExchange.Profiling.Internal;

namespace CBWEB.Controllers
{
    public class ProductController : Umbraco.Cms.Web.Website.Controllers.SurfaceController
    {
        public ProductController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
        }

        [HttpGet]
        public IActionResult Get(Product product)
        {
            bool success = true;
            string message_JsonResult = "Data has been saved successfully.";
            List<ProdctDataItem> filteredProducts = new List<ProdctDataItem>();
            try
            {
                IPublishedContent products = UmbracoContext.Content.GetAtRoot().FirstOrDefault().Children.Where(x => x.Id == (int)((DocumentType)Enum.Parse(typeof(DocumentType), DocumentType.product.ToString()))).FirstOrDefault();
                IPublishedContent productCategories = products.Children.Where(x => x.Name == product.PageName).FirstOrDefault();

                if (productCategories != null)
                {
                    IEnumerable<IPublishedContent> productdetail = productCategories.Children;
                    if (productdetail.Any())
                    {
                        var allProducts = productdetail.Select(s => new ProdctDataItem
                        {
                            Image = s.GetProperty("mainImage").GetValue().ToString(),
                            Name = s.Name,
                            Link = !string.IsNullOrWhiteSpace(s.Url()) ? s.Url() : "#",
                            Heading = s.GetProperty("heading").GetValue().ToString(),
                            Description = s.GetProperty("description").GetValue().ToString(),
                            Size = (IEnumerable<string>)s.GetProperty("size").GetValue(),
                            Color = (IEnumerable<string>)s.GetProperty("color").GetValue(),
                        }).ToList();
                        if (product.Color.HasValue())
                        {
                            if (product.Size.HasValue())
                            {
                                filteredProducts = allProducts.Where(x => x.Color.Contains(product.Color) && x.Size.Contains(product.Size)).ToList();
                            }
                            else
                            {
                                filteredProducts = allProducts.Where(x => x.Color.Contains(product.Color)).ToList();
                            }
                        }
                        else if (product.Size.HasValue())
                        {
                            filteredProducts = allProducts.Where(x => x.Size.Contains(product.Size)).ToList();
                        }
                        else
                        {
                            filteredProducts = allProducts;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message_JsonResult = ex.Message;
            }
            return Json(new
            {
                Success = success,
                Message = message_JsonResult,
                Products = filteredProducts
            });
        }
    }

    public class Product

    {
        public int Id { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string PageName { get; set; }
    }

    public class ProdctDataItem
    {
        public string Image { get; set; }
        public string Link { get; set; }
        public string Name { get; set; }
        public string Heading { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Color { get; set; }
        public IEnumerable<string> Size { get; set; }
    }
}
