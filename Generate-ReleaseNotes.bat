rem https://github.com/StefH/GitHubReleaseNotes

SET version=0.5.0

C:\Dev\GitHub\GitHubReleaseNotes\src\GitHubReleaseNotes\bin\Debug\net8.0\win-x64\GitHubReleaseNotes --output ReleaseNotes.md --skip-empty-releases --exclude-labels env question invalid documentation --version %version% --token %GH_TOKEN%

C:\Dev\GitHub\GitHubReleaseNotes\src\GitHubReleaseNotes\bin\Debug\net8.0\win-x64\GitHubReleaseNotes --output PackageReleaseNotes.txt --skip-empty-releases --exclude-labels env question invalid documentation --template PackageReleaseNotes.template --version %version% --token %GH_TOKEN%