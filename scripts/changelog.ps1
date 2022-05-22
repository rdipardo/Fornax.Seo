#!powershell.exe -File
$HEAD=$(git describe --always)
$PREV_REF=$(git describe --always "${HEAD}^")
$PREV=$(git describe --always --abbrev=0 $PREV_REF)
powershell -NoLogo -NoProfile -c "git log --no-decorate --oneline $PREV..$HEAD"
