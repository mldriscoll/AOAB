$Version = Read-Host "Please enter the new git tag"

function Update-CSPROJ-Version {
	param (
		$file,
		$version
		)
	$xml = New-Object -TypeName XML
	$xml.Load($file)

	$element = $xml.SelectSingleNode("//Version")
	$element.InnerText = $Version

	$streamWriter = New-Object System.IO.StreamWriter($file, $false)
	$xml.Save($streamWriter)
	$streamWriter.Close()
	
	git add $file
}

Update-CSPROJ-Version -file 'AOABO/AOABO.csproj' -version $Version
Update-CSPROJ-Version -file 'OBB-WPF/OBB-WPF.csproj' -version $Version

$message = "Release Version " + $Version

git commit -m $message
git tag $Version