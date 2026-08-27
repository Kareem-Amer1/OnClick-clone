using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Services
{
    public interface IRouteTimeCalculator
    {
        /// <summary>
        /// Calculates the total route time in minutes for a delivery
        /// </summary>
        /// <param name="deliveryCoordinates">Coordinates of the delivery person</param>
        /// <param name="brandCoordinates">List of coordinates for each restaurant/brand in the order</param>
        /// <param name="customerCoordinates">Coordinates of the customer</param>
        /// <returns>Estimated route time in minutes</returns>
        Task<double> CalculateRouteTimeInMinutesAsync(
            (double Latitude, double Longitude) deliveryCoordinates,
            List<(double Latitude, double Longitude)> brandCoordinates,
            (double Latitude, double Longitude) customerCoordinates);

        /// <summary>
        /// Calculates the delivery cost based on route time and hourly rate
        /// </summary>
        /// <param name="routeTimeInMinutes">Route time in minutes</param>
        /// <param name="hourlyRate">The hourly rate (Cost) of the delivery person</param>
        /// <returns>The calculated delivery cost</returns>
        decimal CalculateDeliveryCost(double routeTimeInMinutes, decimal hourlyRate);
    }
} 