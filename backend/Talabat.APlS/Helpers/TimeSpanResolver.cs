using AutoMapper;
using Talabat.APlS.DTOs;
using Talabat.Core.Entites;
using System;

namespace Talabat.APlS.Helpers
{
    public class TimeSpanToStringResolver : IValueResolver<ProductBrand, BrandToReturnDto, string>
    {
        public string Resolve(ProductBrand source, BrandToReturnDto destination, string destMember, ResolutionContext context)
        {
            if (source == null)
                return null;
                
            // Convert TimeSpan to HH:MM format
            try
            {
                return source.OpeningTime.ToString(@"hh\:mm");
            }
            catch (Exception)
            {
                return "00:00"; // Return default value in case of error
            }
        }
    }
    
    public class ClosingTimeResolver : IValueResolver<ProductBrand, BrandToReturnDto, string>
    {
        public string Resolve(ProductBrand source, BrandToReturnDto destination, string destMember, ResolutionContext context)
        {
            if (source == null)
                return null;
                
            // Convert TimeSpan to HH:MM format
            try
            {
                return source.ClosingTime.ToString(@"hh\:mm");
            }
            catch (Exception)
            {
                return "00:00"; // Return default value in case of error
            }
        }
    }
} 