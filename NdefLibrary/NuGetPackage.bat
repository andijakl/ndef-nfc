@echo --------------------------------------------------------------------------
@echo Remember to update the release notes in /NdefLibrary/NdefLibrary.nuspec
@echo --------------------------------------------------------------------------
nuget pack ./NdefLibrary.nuspec -OutputDirectory ./nupkg -Build -Symbols -Prop Configuration=Release
nuget pack ./NdefLibraryExtension.nuspec -OutputDirectory ./nupkg -Build -Symbols -Prop Configuration=Release


@rem nuget pack ./NdefLibrary/NdefLibrary/NdefLibrary.csproj -Symbols -Prop Configuration=Release
@rem http://docs.nuget.org/docs/creating-packages/creating-and-publishing-a-package

