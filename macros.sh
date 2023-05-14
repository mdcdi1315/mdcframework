
# This Script file will run the dotnet command in the .NET workload
# That is , copying the libraries.

echo "Copying required Items..."

if [[ $1 == "" ]]; then
  echo "ERROR: The path to the output directory is not specified."
  echo "INFO: Aborting..."
  exit 4
fi

if [[ -x "$1/zstd.dll" ]]; then
  echo "INFO: No need to copy zstd because it exists."
else
  echo "INFO: Copying zstd.dll..."
  cp "./Imports/zstd.dll" "$PWD/$1/zstd.dll"
  echo "INFO: File copied."
fi

if [[ -x "$1/xxhash.dll" ]]; then
  echo "INFO: No need to copy xxhash because it exists."
else
  echo "INFO: Copying xxhash.dll..."
  cp "./Imports/xxhash.dll" "$PWD/$1/xxhash.dll"
  echo "INFO: File copied."
fi

exit 0
