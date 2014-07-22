TfsFileHistoryImage
===================

Animated GIF of a files TFS history

When working in an established code base sometimes your forced to bear witness to multi-thousand line source code files. What story could the file tell, back to when it was first added, did it gradually mutate into the beast it is today, are there imprints of any heavy footed steps and did anyone try take the reins?

It’d be easy to get downhearted, but what you might be missing are files who’s recent history are the verse. A once feral file might have been tamed overtime with hard work in to an innocuous looking 200 liner which doesn’t raise an eyebrow.

To help quickly visualise this I’ve written a console app which creates and opens an animated GIF showing each revision going forwards in time. Its parameters can be either a TFS local workspace path, or a server address and path. The former can be set up in Visual Studio as an external tool to run on the currently selected file.
