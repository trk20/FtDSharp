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

echo "[1/5] Locating game DLLs..."
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
echo ""

echo "[2/5] Generating API bindings..."
dotnet run --project FtDSharp.CodeGen
echo "  ✓ Code generation complete"
echo ""

echo "[3/5] Building project..."
dotnet build FtDSharp.csproj -c Release
echo "  ✓ Build complete"
echo ""

echo "[4/5] Staging release artifacts..."
rm -rf dist
mkdir -p dist/FtDSharp/TipOfTheDay
mkdir -p dist/FtDSharp/ExampleScripts
mkdir -p dist/FtDSharp/ScriptProject

echo "  → Copying DLLs..."
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

echo "  → Copying metadata..."
cp header.header dist/FtDSharp/
cp plugin.json dist/FtDSharp/
cp LICENSE.md dist/FtDSharp/
cp README.md dist/FtDSharp/

if [ -d "TipOfTheDay" ]; then
  echo "  → Copying TipOfTheDay..."
  cp -r TipOfTheDay/* dist/FtDSharp/TipOfTheDay/ 2>/dev/null || true
fi

if [ -d "ExampleScripts" ]; then
  echo "  → Copying ExampleScripts..."
  cp ExampleScripts/*.cs dist/FtDSharp/ExampleScripts/ 2>/dev/null || true
fi
if [ -d "ScriptProject" ]; then
  echo "  → Copying ScriptProject..."
  cp ScriptProject/* dist/FtDSharp/ScriptProject/ 2>/dev/null || true
fi
    
echo "  → Creating clone helper..."
cat > dist/FtDSharp/clone-source.sh << 'CLONEOF'
#!/bin/bash
git clone https://github.com/trk20/FtDSharp.git
echo "FtDSharp source cloned. See README.md for build instructions."
CLONEOF
chmod +x dist/FtDSharp/clone-source.sh


echo "[5/5] Creating release archive..."
cd dist
if command -v zip &> /dev/null; then
  zip -r "../FtDSharp-v$VERSION.zip" FtDSharp
  echo "  ✓ Created FtDSharp-v$VERSION.zip (using zip)"
else
  # Windows powershell's Compress-Archive
  powershell -Command "Compress-Archive -Path 'FtDSharp\*' -DestinationPath \"../FtDSharp-v$VERSION.zip\" -Force"
  echo "  ✓ Created FtDSharp-v$VERSION.zip (using Compress-Archive)"
fi
cd ..

rm -rf dist

