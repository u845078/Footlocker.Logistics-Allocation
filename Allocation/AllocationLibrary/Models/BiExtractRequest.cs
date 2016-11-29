using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Footlocker.Logistics.Allocation.Factories;

namespace Footlocker.Logistics.Allocation.Models
{
    /// <summary>
    /// Provides an object representation of a business intelligence extract request.
    /// </summary>
    public sealed class BiExtractRequest
    {
        /// <summary>
        /// Defines the request types.
        /// </summary>
        public enum RequestType
        {
            /// <summary>
            /// The expedite purchase orders request.
            /// </summary>
            ExpeditePos,

            /// <summary>
            /// The ring fences request.
            /// </summary>
            RingFences,

            /// <summary>
            /// The range plans request.
            /// </summary>
            RangePlans,

            /// <summary>
            /// The store ranges request.
            /// </summary>
            StoreRanges,

            /// <summary>
            /// The holds request.
            /// </summary>
            Holds,

            /// <summary>
            /// The direct to store stock keeping units request.
            /// </summary>
            DirectToStoreSkus,

            /// <summary>
            /// The direct to store constraints request.
            /// </summary>
            DirectToStoreConstraints,

            /// <summary>
            /// The store seasonality request.
            /// </summary>
            StoreSeasonality,

            /// <summary>
            /// The requested distribution quantities request.
            /// </summary>
            Rdqs,

            /// <summary>
            /// The store back to school request.
            /// </summary>
            StoreBts,

            /// <summary>
            /// The requested order quantities request.
            /// </summary>
            Foqs
        }

        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        public RequestType Request { get; private set; }

        /// <summary>
        /// Gets or sets the factory.
        /// </summary>
        private IBiExtractFactory<BiExtract> Factory { get; set; }

        /// <summary>
        /// Initializes a new instance of the BiExtractRequest class.
        /// </summary>
        /// <param name="request">The request type.</param>
        public BiExtractRequest(RequestType request)
        {
            this.Request = request;
            this.Factory = null;
        }

        /// <summary>
        /// Initializes a new instance of the BiExtractRequest class.
        /// </summary>
        /// <param name="request">The request type.</param>
        public BiExtractRequest(string request)
            : this(ConvertStringToRequestType(request))
        {
        }

        /// <summary>
        /// Convert a string to a request type.
        /// </summary>
        /// <param name="request">The string to be converted.</param>
        /// <returns>The request type corresponding to the specified string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the string contains an unknown request type.</exception>
        private static RequestType ConvertStringToRequestType(string request)
        {
            RequestType returnValue = RequestType.ExpeditePos;

            if (Enum.TryParse<RequestType>(request, true, out returnValue))
            {
            }
            else
            {
                throw new ArgumentOutOfRangeException("request", request, "An unknown request type was specified.");
            }
            return returnValue;
        }

        /// <summary>
        /// Gets the stored procedure name for the current business intelligence extract request.
        /// </summary>
        /// <returns>The store procedure name for the current business intelligence extract request.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown request type is encountered.</exception>
        internal string GetStoredProcedureName()
        {
            string returnValue = String.Empty;

            switch (this.Request)
            {
                case RequestType.ExpeditePos:
                    returnValue = "dbo.getExpeditePOsBIExtract";
                    break;
                case RequestType.RingFences:
                    returnValue = "dbo.getRingFencesBIExtract";
                    break;
                case RequestType.RangePlans:
                    returnValue = "dbo.getRangePlansBIExtract";
                    break;
                case RequestType.StoreRanges:
                    returnValue = "dbo.getStoreRangesBIExtract";
                    break;
                case RequestType.Holds:
                    returnValue = "dbo.getHoldsBIExtract";
                    break;
                case RequestType.DirectToStoreSkus:
                    returnValue = "dbo.getDirectToStoreSkusBIExtract";
                    break;
                case RequestType.DirectToStoreConstraints:
                    returnValue = "dbo.getDirectToStoreConstraintsBIExtract";
                    break;
                case RequestType.StoreSeasonality:
                    returnValue = "dbo.getStoreSeasonalityBIExtract";
                    break;
                case RequestType.Rdqs:
                    returnValue = "dbo.getRDQsBIExtract";
                    break;
                case RequestType.StoreBts:
                    returnValue = "dbo.getStoreBTSBIExtract";
                    break;
                case RequestType.Foqs:
                    returnValue = "dbo.getFOQBIExtract";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("this.Request", this.Request
                        , "An unknown request type has been encountered.");
            }
            return returnValue;
        }

        /// <summary>
        /// Get the business intelligence extract factory.
        /// </summary>
        /// <returns>The business intelligence extract factory.</returns>
        internal IBiExtractFactory<BiExtract> GetFactory()
        {
            if (this.Factory == null)
            {
                switch (this.Request)
                {
                    case RequestType.ExpeditePos:
                        this.Factory = new ExpeditePOFactory();
                        break;
                    case RequestType.RingFences:
                        this.Factory = new RingFenceFactory();
                        break;
                    case RequestType.RangePlans:
                        this.Factory = new RangePlanFactory();
                        break;
                    case RequestType.StoreRanges:
                        this.Factory = new StoreRangeFactory();
                        break;
                    case RequestType.Holds:
                        this.Factory = new HoldFactory();
                        break;
                    case RequestType.DirectToStoreSkus:
                        this.Factory = new DirectToStoreSkuBIExtractFactory();
                        break;
                    case RequestType.DirectToStoreConstraints:
                        this.Factory = new DirectToStoreConstraintFactory();
                        break;
                    case RequestType.StoreSeasonality:
                        this.Factory = new StoreSeasonalityGroupFactory();
                        break;
                    case RequestType.Rdqs:
                        this.Factory = new RDQExtractFactory();
                        break;
                    case RequestType.StoreBts:
                        this.Factory = new StoreBTSExtractFactory();
                        break;
                    case RequestType.Foqs:
                        this.Factory = new FoqExtractFactory();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("this.Request", this.Request
                            , "An unknown request type was encountered.");
                }
            }
            return this.Factory;
        }
    }
}
