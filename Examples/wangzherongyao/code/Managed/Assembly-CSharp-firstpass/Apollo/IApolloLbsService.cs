namespace Apollo
{
    using System;

    public interface IApolloLbsService : IApolloServiceBase
    {
        event OnLocationNotifyHandle onLocationEvent;

        bool CleanLocation();
        void GetNearbyPersonInfo();
    }
}

