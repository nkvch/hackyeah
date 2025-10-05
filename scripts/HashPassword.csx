#!/usr/bin/env dotnet-script
#r "nuget: BCrypt.Net-Next, 4.0.3"

using BCrypt.Net;

if (Args.Count == 0)
{
    Console.WriteLine("Usage: dotnet script HashPassword.csx <password>");
    return;
}

var password = Args[0];
var workFactor = 12;
var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor);

Console.WriteLine($"Password: {password}");
Console.WriteLine($"Work Factor: {workFactor}");
Console.WriteLine($"Hash: {hash}");

