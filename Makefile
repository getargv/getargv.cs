.PHONY: build test clean run pack sign publish

build:
	dotnet build
test:
	dotnet test
clean:
	dotnet clean
	find . \( -name bin -o -name obj \) -delete

run:
	dotnet run --project Getargv.Tool

pack:
	dotnet pack Getargv

sign: pack
	dotnet nuget sign ./Getargv/bin/Release/Getargv.*.nupkg --certificate-path $(PathToTheCertificate) --timestamper $(TimestampServiceURL)

#publish: sign
publish: pack
	dotnet nuget push ./Getargv/bin/Release/Getargv.*.nupkg --api-key $$NUGET_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate
