using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footlocker.Logistics.Allocation.Models
{
    /// <summary>
    /// Provides an object representation of a Quantum Instance.
    /// </summary>
    public sealed class QuantumInstance
    {
        /// <summary>
        /// Defines the instance identifiers.
        /// </summary>
        public enum InstanceId
        {
            /// <summary>
            /// The Footlocker/Kids/Lady/Footaction instance identifier.
            /// </summary>
            Lockers = 1,

            /// <summary>
            /// The Champs instance identifier.
            /// </summary>
            Champs = 2,

            /// <summary>
            /// The European instance identifier.
            /// </summary>
            Europe = 3,

            /// <summary>
            /// The Asia/Pacific instance identifier.
            /// </summary>
            AsiaPacific = 4

        }

        /// <summary>
        /// Defines the Lockers interface identifier.
        /// </summary>
        private const string InterfaceIdentifierLockers = "LK";

        /// <summary>
        /// Defines the Champs interface identifier.
        /// </summary>
        private const string InterfaceIdentifierChamps = "CH";


        /// <summary>
        /// Defines the European interface identifier.
        /// </summary>
        private const string InterfaceIdentifierEurope = "EU";

        /// <summary>
        /// Defines the Asia/Pacific interface identifier.
        /// </summary>
        private const string InterfaceIdentifierAsiaPacific = "AP";

        /// <summary>
        /// Gets or sets the Quantum instance identifier.
        /// </summary>
        public InstanceId Id { get; private set; }

        /// <summary>
        /// Gets the interface identifier.
        /// </summary>
        public string InterfaceId
        {
            get
            {
                string returnValue = String.Empty;

                switch (this.Id)
                {
                    case InstanceId.Lockers:
                        returnValue = QuantumInstance.InterfaceIdentifierLockers;
                        break;
                    case InstanceId.Champs:
                        returnValue = QuantumInstance.InterfaceIdentifierChamps;
                        break;
                    case InstanceId.AsiaPacific:
                        returnValue = QuantumInstance.InterfaceIdentifierAsiaPacific;
                        break;
                    case InstanceId.Europe:
                        returnValue = QuantumInstance.InterfaceIdentifierEurope;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("this.Id", this.Id
                            , "An unknown Quantum Instance was encoutered.");
                }
                return returnValue;
            }
        }

        /// <summary>
        /// Initializes a new instance of the QuantumInstance class.
        /// </summary>
        /// <param name="id">The initial value for the Id property.</param>
        public QuantumInstance(InstanceId id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the QuantumInstance class.
        /// </summary>
        /// <param name="id">The initial value for the Id property.</param>
        public QuantumInstance(int id)
            : this(QuantumInstance.ConvertIntToInstanceId(id))
        {
        }

        /// <summary>
        /// Initializes a new instance of the QuantumInstance class.
        /// </summary>
        /// <param name="id">The initial value for the Id property.</param>
        public QuantumInstance(string id)
            : this(QuantumInstance.ConvertStringToInt(id))
        {
        }

        /// <summary>
        /// Convert an integer to an instance identifier.
        /// </summary>
        /// <param name="value">The integer to be converted.</param>
        /// <returns>The instance identifier from the conversion.</returns>
        /// <exception cref="ArgumentOutOfRange">Thrown when the specified value does not correspond to an instance identifier.</exception>
        private static InstanceId ConvertIntToInstanceId(int value)
        {
            InstanceId returnValue = InstanceId.Lockers;

            if (Enum.IsDefined(typeof(InstanceId), value))
            {
                returnValue = (InstanceId)value;
            }
            else
            {
                throw new ArgumentOutOfRangeException("value", value, "An unknown instance identifier was specified.");
            }
            return returnValue;
        }

        /// <summary>
        /// Convert a string to an integer.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The integer from the conversion.</returns>
        /// <exception cref="ArgumentException">Thrown when the string is not a natural number.</exception>
        private static int ConvertStringToInt(string value)
        {
            int returnValue = 0;

            if (Int32.TryParse(value, out returnValue))
            {
            }
            else
            {
                throw new ArgumentException(
                    "A invalid instance identifier was specified.  The instance identifier must be a natural number."
                    , value);
            }
            return returnValue;
        }

        /// <summary>
        /// Determine if the current Quantum Instance is the specified instance identifier.
        /// </summary>
        /// <param name="value">The instance identifier for the comparison.</param>
        /// <returns>A value indicating whether (true) or not (false) the current Quantum Instance is the specified instance.</returns>
        public bool Is(InstanceId value)
        {
            return this.Id == value;
        }

        /// <summary>
        /// Determine if the current Quantum Instance is North America.
        /// </summary>
        /// <returns>A value indicating whether (true) or not (false) the current Quantum Instance is North America.</returns>
        public bool IsNorthAmerica()
        {
            return ((this.Is(InstanceId.Lockers)) || (this.Is(InstanceId.Champs)));
        }

        /// <summary>
        /// Determine if the current Quantum Instance is Asia/Pacific.
        /// </summary>
        /// <returns>A value indicating whether (true) or not (false) the current Quantum Instance is Asia/Pacific.</returns>
        public bool IsAsiaPacific()
        {
            return this.Is(InstanceId.AsiaPacific);
        }

        /// <summary>
        /// Determine if the current Quantum Instance is Europe.
        /// </summary>
        /// <returns>A value indicating whether (true) or not (false) the current Quantum Instance is Europe.</returns>
        public bool IsEurope()
        {
            return this.Is(InstanceId.Europe);
        }

        /// <summary>
        /// Convert the current Quantum Instance to a string.
        /// </summary>
        /// <returns>The string equivalent of the current Quantum Instance.</returns>
        public override string ToString()
        {
            string returnValue = String.Empty;

            switch (this.Id)
            {
                case InstanceId.Lockers:
                    returnValue = "Lockers";
                    break;
                case InstanceId.Champs:
                    returnValue = "Champs";
                    break;
                case InstanceId.AsiaPacific:
                    returnValue = "Asia/Pacific";
                    break;
                case InstanceId.Europe:
                    returnValue = "Europe";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("this.Id", this.Id
                        , "An unknown instance identifier has been encountered.");
            }
            return returnValue;
        }
    }
}
