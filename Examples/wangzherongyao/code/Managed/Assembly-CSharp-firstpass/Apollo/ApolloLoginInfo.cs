namespace Apollo
{
    using System;

    public class ApolloLoginInfo : ApolloStruct<ApolloLoginInfo>
    {
        public ApolloAccountInfo AccountInfo;
        public ApolloServerRouteInfo ServerInfo;
        public ApolloWaitingInfo WaitingInfo;

        public override ApolloLoginInfo FromString(string src)
        {
            ApolloStringParser parser = new ApolloStringParser(src);
            this.AccountInfo = parser.GetObject<ApolloAccountInfo>("AccountInfo");
            this.WaitingInfo = parser.GetObject<ApolloWaitingInfo>("WaitingInfo");
            string data = parser.GetString("ServerInfo");
            if (data != null)
            {
                this.ServerInfo = new ApolloServerRouteInfo();
                this.ServerInfo.FromString(data);
            }
            return this;
        }
    }
}

