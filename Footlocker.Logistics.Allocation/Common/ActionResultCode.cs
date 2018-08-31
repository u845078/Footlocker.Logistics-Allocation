using System;

namespace Footlocker.Logistics.Allocation.Common
{
    public enum ActionResultCode
    {
        Success = -1,
        SystemError = 1,
        ValidationError = 2,
        DeleteConstraintError = 3
    }
}
