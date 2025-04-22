extends Node2D


# Called when the node enters the scene tree for the first time.
static func add_key_binding(action: String, key: int) -> void:
	var event = InputEventKey.new() 
	event.keycode  = key 
	if not InputMap.has_action(action): 
		InputMap.add_action(action) 
	InputMap.action_add_event(action,  event)

func _ready():
	# 动态设置输入映射的代码 
	add_key_binding("fire_small_wave", KEY_Q)
	add_key_binding("fire_medium_wave", KEY_W)
	add_key_binding("fire_large_wave", KEY_E)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
