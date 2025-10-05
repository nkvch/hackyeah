#!/bin/bash

# Fix LoginCommand usage (record with positional parameters)
cd /Users/stanislaustankevich/projects/hackyeah/src/Backend/Tests/UknfPlatform.UnitTests

# Fix LoginCommandHandlerTests
sed -i '' 's/var command = new LoginCommand$/var command = new LoginCommand(/g' Application/Auth/LoginCommandHandlerTests.cs
sed -i '' 's/^        {$//' Application/Auth/LoginCommandHandlerTests.cs  
sed -i '' 's/            Email = "\([^"]*\)",$/            "\1",/g' Application/Auth/LoginCommandHandlerTests.cs
sed -i '' 's/            Password = "\([^"]*\)"$/            "\1"/g' Application/Auth/LoginCommandHandlerTests.cs
sed -i '' 's/        };$/        );/g' Application/Auth/LoginCommandHandlerTests.cs

# Fix User.Create -> User.CreateExternal
sed -i '' 's/User\.Create(/User.CreateExternal(/g' Application/Auth/LoginCommandHandlerTests.cs
sed -i '' 's/User\.Create(/User.CreateExternal(/g' Infrastructure/Identity/JwtTokenServiceTests.cs

# Fix UserType enum values
sed -i '' 's/UserType\.ExternalUser/UserType.External/g' Application/Auth/LoginCommandHandlerTests.cs
sed -i '' 's/UserType\.ExternalUser/UserType.External/g' Infrastructure/Identity/JwtTokenServiceTests.cs
sed-i '' 's/UserType\.UknfUser/UserType.Internal/g' Infrastructure/Identity/JwtTokenServiceTests.cs

echo "Tests fixed!"

