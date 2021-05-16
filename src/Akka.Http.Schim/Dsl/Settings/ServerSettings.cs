﻿using System;
using System.Net;
using Akka.Actor;
using Akka.Annotations;
using Akka.Configuration;

namespace Akka.Http.Dsl.Settings
{
    [InternalApi]
    public sealed class ServerSettings
    {
        public string ServerHeader { get; }
        public bool RemoteAddressAttribute { get; }
        public int DefaultHttpPort { get; }
        public int DefaultHttpsPort { get; }
        public int TerminationDeadlineExceededResponse { get; }

        public static ServerSettings Create(ExtendedActorSystem system)
        {
            // TODO
            // var c = system.Settings.Config.GetConfig("akka.http.server");
            //
            // return new ServerSettings(
            //     c.GetString("server-header"),
            //     c.GetBoolean("remote-address-attribute"),
            //     c.GetInt("default-http-port"),
            //     c.GetInt("default-https-port"),
            //     TerminationDeadlineExceededResponseFrom(c));

            return new ServerSettings("", false, 80, 443, 503);
        }

        private ServerSettings(string serverHeader, bool remoteAddressAttribute, int defaultHttpPort, int defaultHttpsPort, int terminationDeadlineExceededResponse)
        {
            ServerHeader = serverHeader;
            RemoteAddressAttribute = remoteAddressAttribute;
            DefaultHttpPort = defaultHttpPort;
            DefaultHttpsPort = defaultHttpsPort;
            TerminationDeadlineExceededResponse = terminationDeadlineExceededResponse;
        }

        private static int TerminationDeadlineExceededResponseFrom(Config c)
        {
            var status = c.GetInt("termination-deadline-exceeded-response.status");
            if (!Enum.IsDefined(typeof(HttpStatusCode), status))
            {
                throw new ArgumentException($"Illegal status code set for `termination-deadline-exceeded-response.status`, was: [{status}]");
            }
            return status;
        }
    }
}