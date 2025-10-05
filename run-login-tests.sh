#!/bin/bash

echo "🧪 Running Story 1.4 Login Tests..."
echo "=================================="
echo ""

cd /Users/stanislaustankevich/projects/hackyeah/src/Backend

echo "📦 Restoring packages..."
docker run --rm -v "$(pwd):/src" -w /src mcr.microsoft.com/dotnet/sdk:8.0 dotnet restore Tests/UknfPlatform.UnitTests/UknfPlatform.UnitTests.csproj

echo ""
echo "🔨 Building test project..."
docker run --rm -v "$(pwd):/src" -w /src mcr.microsoft.com/dotnet/sdk:8.0 dotnet build Tests/UknfPlatform.UnitTests/UknfPlatform.UnitTests.csproj --no-restore

echo ""
echo "✅ Running LoginCommandHandlerTests..."
docker run --rm -v "$(pwd):/src" -w /src mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet test Tests/UknfPlatform.UnitTests/UknfPlatform.UnitTests.csproj \
  --filter "FullyQualifiedName~LoginCommandHandlerTests" \
  --no-build --verbosity normal

echo ""
echo "✅ Running JwtTokenServiceTests..."
docker run --rm -v "$(pwd):/src" -w /src mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet test Tests/UknfPlatform.UnitTests/UknfPlatform.UnitTests.csproj \
  --filter "FullyQualifiedName~JwtTokenServiceTests" \
  --no-build --verbosity normal

echo ""
echo "🎉 Test execution complete!"

