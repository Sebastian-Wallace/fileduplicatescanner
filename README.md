# fileduplicatescanner
A simple console app file duplication scanner.

## Purpose
This is principally designed to scan small to medium sized folders for duplicate files. It compares the names and then the hash value of the files to the first encountered file of the same name. If they are the same, it markes them as duplicate and adds them to a dictionary, this dictionary can then be iterated through to delete all duplicates. 

## Limitations
If the program is not run with admin privilages, it will fail to delete or read certain systems or protected files.

When using the delete function, the program does not take into account folders it has inadvertantly emptied.

## Compile and run 
Open project in Visual studio and build. There are not independent librarys used, only native librarys so it should compile without issue. 

## Usage
You are free to use for commercial and personal use. I am not responsible for loss, damage or corruption of files while using this code. The code is supplied as is and as such has no warrenty.
