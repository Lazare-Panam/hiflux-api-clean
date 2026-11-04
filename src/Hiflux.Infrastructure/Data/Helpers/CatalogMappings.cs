using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Hiflux.Core.Dtos;
using Hiflux.Core.Entities.Products;

namespace Hiflux.Infrastructure.Data.Helpers;

public class CatalogMappings
{
    public static readonly Expression<Func<CatalogProduct, CatalogProductDto>> ToDto = p => new CatalogProductDto
    {
        ProductId = p.ProductId,
        PartNumber = p.PartNumber,
        ProductStatus = p.ProductStatus,
        FamilyDescription = p.FamilyDescription,
        DetailDescription = p.DetailDescription,
        ImageUrl = p.ImageUrl,
        Price = p.Price,
        Specs = p.FilterValues
               .Select(v => new ProductSpecDto { Filter = v.Filter.Name, Value = v.Value })
               .ToList()
        
    };
    public static CategoryDto MapCategory(Category c) => new()
    {
        CategoryId = c.CategoryId,
        Name = c.Name,
        CatalogUrl = c.CatalogUrl,
        CadUrl = c.CadUrl,
        Description = c.Description,
        BodyMaterial = c.BodyMaterial,
        SeatMaterial = c.SeatMaterial,
        Design = c.Design,
        TemperatureRange = c.TemperatureRange,
        Approvals = c.Approvals,
        DatasheetUrl = c.DatasheetUrl,
        KeyFeatures = c.KeyFeatures,
        Certificates = c.Certificates.Select(x => new CertificateDto { Label = x.Label, Url = x.Url }).ToList(),
    };
}
