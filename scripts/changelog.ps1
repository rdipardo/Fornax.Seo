#!powershell.exe -File
$HEAD=$(git describe --always)
$PREV_REF=$(git describe --always "${HEAD}^")
$PREV=$(git describe --always --abbrev=0 $PREV_REF)
powershell -NoLogo -NoProfile -c "git log --pretty='format:%h%x09%s' --no-merges -P -i --invert-grep --grep='(?|action|dependabot)' $PREV..$HEAD"
