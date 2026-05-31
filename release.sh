#!/bin/bash
set -e

VERSION=""
PRERELEASE=true

while [[ $# -gt 0 ]]; do
  case $1 in
    -v|--version)
      VERSION="$2"
      shift 2
      ;;
    --stable)
      PRERELEASE=false
      shift
      ;;
    *)
      echo "Usage: ./release.sh -v VERSION [--stable]"
      echo "Example: ./release.sh -v 0.2.0"
      echo "         ./release.sh -v 0.2.0 --stable"
      exit 1
      ;;
  esac
done

if [ -z "$VERSION" ]; then
  echo "Error: Version required"
  echo "Usage: ./release.sh -v VERSION [--stable]"
  exit 1
fi

echo "[1/6] Locating game DLLs..."
search_paths=(
  "/c/Program Files (x86)/Steam/steamapps/common/From The Depths/From_The_Depths_Data/Managed"
  "/c/Program Files/Steam/steamapps/common/From The Depths/From_The_Depths_Data/Managed"
  "$HOME/Documents/From The Depths/From_The_Depths_Data/Managed"
  "$HOME/.steam/steam/steamapps/common/From The Depths/From_The_Depths_Data/Managed"
  "$HOME/.local/share/Steam/steamapps/common/From The Depths/From_The_Depths_Data/Managed"
  "ftd-managed"
)

managed_path=""
for path in "${search_paths[@]}"; do
  if [ -d "$path" ] && [ -n "$(ls "$path"/*.dll 2>/dev/null)" ]; then
    managed_path="$path"
    echo "  ✓ Found at: $managed_path"
    break
  fi
done

if [ -z "$managed_path" ]; then
  echo "  ✗ Error: Could not find From The Depths Managed DLLs"
  echo ""
  echo "Searched locations:"
  for path in "${search_paths[@]}"; do
    echo "  - $path"
  done
  echo ""
  echo "Please either:"
  echo "  1. Install From The Depths via Steam"
  echo "  2. Manually copy DLLs to ftd-managed/"
  exit 1
fi

if [ "$managed_path" != "ftd-managed" ]; then
  echo "  → Copying DLLs to ftd-managed/..."
  mkdir -p ftd-managed
  cp "$managed_path"/*.dll ftd-managed/
fi

dll_count=$(ls ftd-managed/*.dll 2>/dev/null | wc -l)
echo "  ✓ $dll_count DLLs ready"

echo "  → Preparing IDE reference assemblies..."
mkdir -p References
if [ ! -f "References/UnityEngine.CoreModule.dll" ]; then
  cp ftd-managed/UnityEngine.CoreModule.dll References/
fi
echo "  ✓ References/UnityEngine.CoreModule.dll ready"
echo ""

echo "[2/6] Generating API bindings..."
dotnet run --project FtDSharp.CodeGen
echo "  ✓ Code generation complete"
echo ""

echo "[3/6] Building project..."
dotnet build FtDSharp.csproj -c Release
echo "  ✓ Build complete"
echo ""

echo "[4/6] Staging release artifacts..."
rm -rf dist
mkdir -p dist/FtDSharp/TipOfTheDay
mkdir -p dist/FtDSharp/ExampleScripts/Scripts
mkdir -p dist/FtDSharp/ScriptProject
mkdir -p dist/FtDSharp/References

echo "  → Copying mod DLLs..."
dlls=(
  "0Harmony.dll"
  "Microsoft.CodeAnalysis.dll"
  "Microsoft.CodeAnalysis.CSharp.dll"
  "System.Collections.Immutable.dll"
  "System.Reflection.Metadata.dll"
  "System.Text.Encoding.CodePages.dll"
  "System.Memory.dll"
  "System.Runtime.CompilerServices.Unsafe.dll"
  "System.Numerics.Vectors.dll"
  "System.Threading.Tasks.Extensions.dll"
  "Microsoft.CodeAnalysis.BannedApiAnalyzers.dll"
  "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll"
  "FtDSharp.API.dll"
  "FtDSharp.dll"
)

for dll in "${dlls[@]}"; do
  if [ -f "$dll" ]; then
    cp "$dll" dist/FtDSharp/
    echo "    ✓ $dll"
  else
    echo "    ✗ Missing: $dll"
    exit 1
  fi
done

echo "  → Copying IDE reference assemblies..."
cp References/UnityEngine.CoreModule.dll dist/FtDSharp/References/
echo "    ✓ References/UnityEngine.CoreModule.dll"

echo "  → Copying metadata..."
cp header.header dist/FtDSharp/
cp plugin.json dist/FtDSharp/
cp LICENSE.md dist/FtDSharp/
cp README.md dist/FtDSharp/

if [ -d "TipOfTheDay" ]; then
  echo "  → Copying TipOfTheDay..."
  cp -r TipOfTheDay/* dist/FtDSharp/TipOfTheDay/ 2>/dev/null || true
fi

echo "  → Copying ScriptProject..."
cp ScriptProject/FtDSharpScript.csproj dist/FtDSharp/ScriptProject/
cp ScriptProject/MyScript.cs dist/FtDSharp/ScriptProject/
cp ScriptProject/README.md dist/FtDSharp/ScriptProject/

echo "  → Copying ExampleScripts..."
cp ExampleScripts/ExampleScripts.csproj dist/FtDSharp/ExampleScripts/
cp ExampleScripts/README.md dist/FtDSharp/ExampleScripts/
cp ExampleScripts/Scripts/*.cs dist/FtDSharp/ExampleScripts/Scripts/

example_count=$(ls dist/FtDSharp/ExampleScripts/Scripts/*.cs 2>/dev/null | wc -l)
if [ "$example_count" -eq 0 ]; then
  echo "    ✗ No example scripts found in ExampleScripts/Scripts/"
  exit 1
fi
echo "    ✓ $example_count example scripts"

echo "  → Verifying IDE projects build..."
dotnet build dist/FtDSharp/ScriptProject/FtDSharpScript.csproj -v q
dotnet build dist/FtDSharp/ExampleScripts/ExampleScripts.csproj -v q
echo "    ✓ ScriptProject and ExampleScripts compile"

echo "  → Creating clone helper..."
cat > dist/FtDSharp/clone-source.sh << 'CLONEOF'
#!/bin/bash
git clone https://github.com/trk20/FtDSharp.git
echo "FtDSharp source cloned. See README.md for build instructions."
CLONEOF
chmod +x dist/FtDSharp/clone-source.sh

echo "[5/6] Creating release archive..."
cd dist
if command -v zip &> /dev/null; then
  zip -r "../FtDSharp.zip" FtDSharp
  echo "  ✓ Created FtDSharp.zip (using zip)"
else
  powershell -Command "Compress-Archive -Path 'FtDSharp\*' -DestinationPath \"../FtDSharp.zip\" -Force"
  echo "  ✓ Created FtDSharp.zip (using Compress-Archive)"
fi
cd ..

echo "[6/6] Release summary"
echo "  Version:   $VERSION"
echo "  Prerelease: $PRERELEASE"
echo "  Archive:   FtDSharp.zip"
echo ""
echo "Done. Upload FtDSharp.zip to GitHub Releases and tag v$VERSION when ready."

rm -rf dist
