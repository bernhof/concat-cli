# Filer
Command line utiliy that performs various mildly useful file operations.

## Usage

### concat
Concatenates multiple files, optionally including a separator (`-s`) between each file and/or a header (`-h`) before each file.

Use `\n` for new lines and `\t` for tabs.

```
filer concat *.sql -s "\ngo\n" > output.sql
filer concat *.sql !create.sql -s "\ngo\n" -h "\n--{0}\n" > output.sql
```

### delimit
Changes delimiter in a csv file.

Use `\t` to specify tab delimiter.

```
filer delimit semicolon.csv ";" "," > comma.csv
filer delimit tab.csv "\t" "|" > pipe.csv
```

### shift-date-taken
Shifts Exif "Date Taken" property on photo files.

```
filer shift-date-taken *.jpg --shift-hours -8
filer shift-date-taken **\*.jpg --shift-years 1
```