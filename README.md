# AutoDoc
<img src="AutoDoc_Kommentar.png" alt="AutoDoc-Kommentar">

While on vacation during my 2020 bachelor thesis, I developed a tool to reduce the effort of creating comment blocks for Doxygen.
It was used not only by me, but by the whole ACS team in the WMF coffee machine software development department.
The plugin for the Visual Studio IDE parses the method signatures in the header and source files and generates the part of the 
of the comment that can be generated automatically. Previous comments are retained and marked as "CHANGED" before being removed.
