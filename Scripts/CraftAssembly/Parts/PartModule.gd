# Base class for GDscript based part modules to get easy access to various part signals.

extends Node
class_name PartModule

var part_script # Assign this upon part initialization

# Update function controlled by Part.cs
# Key difference to the Process function is that this one does not run if the part is disabled.
func update():
    pass

# Invoked when the part is initialized
func part_init():
    pass