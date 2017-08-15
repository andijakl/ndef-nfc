
vCard Class Library for .NET (Version 0.4; LGPL license)
Copyright (c) 2007-2009 David Pinch * http://www.thoughtproject.com/Libraries/vCard/

Introduction

    This is a simple library for manipulating vCards.  A vCard is used to exchange
    contact information (e.g. to import into Microsoft Outlook or download from
    the web into a suitable PIM application).  All code is released under the LGPL
    for free use and with full permission to use with commercial software.

General Design:

    (a) The vCard object model is separated from the various vCard file formats.
        You can manipulate contact information, photos, and other properties in the
        standard way you would expect from a .NET class library.

    (b) Almost all vCard properties can exist multiple times in a vCard file.
        However, some judgement is used when determining whether a property
        should be implemented as a collection or a singular property.  For
        example, it is possible to have multiple formatted names, each in a 
        different language.  The need for this functionality is very rare
        and therefore the formatted name property is implemented as a string
        property rather than collection of "formatted name" objects.

        On the other hand, multiple email addresses are common.  Email addresses
        are stored in a collection.  Nevertheless, feel free to request changes
        if you believe singular property should be converted to a collection.

    (c) The vCardReader and vCardWriter classes are abstract classes that
        define the interfaces for reading and writing vCard formats.  This
        version of the library supplies vCardStandardReader and vCardStandardWriter,
        which implement the vCard 2.1 format specification.

        A future version will include support for Jabber, RDF and hCard formats.
        If you want to help evolve the library, adding support for those
        formats will be appreciated.

    (d) Example usage (in VB.NET; the library itself is in C#):

          Dim card As New vCard()
 
          card.GivenName = "David"
          card.FamilyName = "Pinch"
        
          Dim photo as New vCardPhoto( _
              "http://www.thoughtproject.com/Common/me.jpg")
 
          card.Photos.Add(photo)

          Dim writer as vCardStandardWriter = New vCardStandardWriter()

          writer.EmbedWebPhotos = true
          writer.Write(card, "c:\davepinch.vcf")


Folders (directories):

    .\ (root)

        file_id.diz ............................. Description in FILE_ID.DIZ format*
        license.txt ............................. LGPL license text
        pad_file.xml ............................ Description in PAD XML format*
        readme.txt .............................. This file.
        roadmap.txt ............................. Plans for the next version

        * These two files are provided for the convenience of file distribution
          networks and shareware/freeware sites.  They contain information that can
          be extracted by special computer programs.  Developers can ignore or
          delete the files.

    .\Solution\

        The Solution folder contains a Visual Studio 2008 solution for building the
        vCard class library.  A unit test project (nUnit) is also provided.

    .\Reference\

        Copies of the vCard RFC (Request for Comments) documents.  Other resources
        and technical information will be placed into this folder in the future
        releases.  Documents in this folder may be copyrighted products under
        licenses that allow redistribution (but not affiliated with the author).

    .\Help\

        A help project for Sandcastle Help File Builder.  See Readme.Text in the
        folder for instructions on building your own documentation.  An sample
        help file (CHM) has been compiled for your convenience.

    .\Samples\

        Samples demonstrating use the library. 


Current Status

    This is version 0.4 of the library.  It implements a substantial portion of the
    vCard standard.  Future efforts will focus on compatible formats like
    hCard and RDF.  However, the library is still treated as beta status.
    Please do not use on production systems unless you are confident with the
    degree of functionality currently implemented.


        ADR .......................... Delivery address
        BDAY ......................... Birthdate (partial)
        CATEGORIES ................... Category names / keywords
        CLASS ........................ Access classification
        EMAIL ........................ Email address
        FN ........................... Formatted name
        GEO .......................... Geographical coordinates
        KEY .......................... Certificate key
        LABEL ........................ Delivery label
        MAILER ....................... Mailer software name
        N ............................ Name
        NAME ......................... Display name of the vCard
        NICKNAME ..................... Nickname
        NOTE ......................... Note/Comment
        ORG .......................... Organization
        PHOTO ........................ Personal photos
        PRODID ....................... Product name/version
        REV .......................... Revision date
        ROLE ......................... Role/profession
        SOURCE ....................... Directory source
        TEL .......................... Telephone number
        TITLE ........................ Job title
        TZ ........................... Time zone
        UID .......................... Unique ID
        URL .......................... Web site
        X-WAB-GENDER ................. Gender (Outlook Extension)

    Known issues to be fixed in a future release (before version 1.0):

        - Preferred addresses and labels not marked as preferred

Credits

    Thanks for Martin Meraner for providing testing assistance and helping with
    the certificate implementation.  Thanks to Bill Lunney for assisting with
    photo/imaging support.  Thanks to Richard Bennett for testing.  Thanks
    to Robbie Paplin for helping with image (photo) support.  Thanks to Karsten
    Januszeski for pushing me to update the library and eventually move to
    CodePlex.


Support and Downloads

    The author is happy to assist people using the library.  Please make sure you have
    downloaded the latest copy of the code from the following web site:      

        http://www.thoughtproject.com/Libraries/vCard/
  
    You can reach the author at the following:

        David Pinch
        http://www.linkedin.com/in/davepinch
        davepinch@gmail.com

    When sending code changes or bug reports, let me know if you want me to list your
    home page or some other personal statement.  This is the least I can do.  People
    who contribute source code and other materials will also be credited in the
    copyrights.  You should be comfortable with the LGPL open source license before
    sharing any code, though!

    Linked-In users are welcome to send an invitation/connection request.  Be sure
    to mention this library (or any library on thoughtproject.com) in your invitation.

    Please submit vCard files that are not correctly parsed by the library.


License and Copyrights

  vCard Class Library for .NET (version 0.4 -- alpha)
  Copyright (C) 2007 David Pinch (dave@thoughtproject.com) and contributors
  
  This library is free software; you can redistribute it and/or
  modify it under the terms of the GNU Lesser General Public
  License as published by the Free Software Foundation; either
  version 2.1 of the License, or (at your option) any later version.

  This library is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
  Lesser General Public License for more details.

  You should have received a copy of the GNU Lesser General Public
  License along with this library; if not, write to the Free Software
  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
  