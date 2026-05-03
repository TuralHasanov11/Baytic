### Init Spec
```sh
specify init --here
```

### Start docker composer with override file

```sh
docker-compose -f docker-compose.yaml -f docker-compose.override.yaml up -d --build
```

### Test
```sh
dotnet test ./Baytic.sln

./run-unit-tests-and-coverage.ps1
./run-integration-tests-and-coverage.ps1
./run-unit-tests-and-coverage.sh
./run-integration-tests-and-coverage.sh
```

### OpenSSF socorecard
```sh
docker run -e GITHUB_AUTH_TOKEN=<your access token> gcr.io/openssf/scorecard:stable --repo=https://github.com/TuralHasanov11/Baytic
```

### Add project to solution
```sh
dotnet sln add ./src/ProjectName/ProjectName.csproj
```