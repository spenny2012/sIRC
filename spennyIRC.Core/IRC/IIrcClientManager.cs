﻿namespace spennyIRC.Core.IRC;

public interface IIrcClientManager
{
    Task ConnectAsync(string server, int port, bool useSsl = false);

    // TODO: introduce constant
    Task QuitAsync(string quitMsg = "Test");

    void SetSession(IIrcSession session);
}