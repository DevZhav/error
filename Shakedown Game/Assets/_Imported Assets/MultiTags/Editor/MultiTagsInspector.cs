using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;

//Base 75 Reimplimentation of the Unity tag system to allow for multiple tags on the same game object.

 
[CustomEditor(typeof(MultiTags))]
public class ObjectBuilderEditor : Editor
{
	 






	public string stringToEdit = "//MultiTags are NOT case sensitive! :)\n\n" +

				"void OnTriggerEnter2D(Collider2D col) \n" +
				"{ \n\n" +
				
				
				@"   if (col.gameObject.HasTag (""Enemy"")) { " +
			
				"\n\n	//Magic here \n\n" +
 			
				"    } \n" +

				"} \n";

	

	


		public string tagDescription;
		public static bool expandGlobals = true;
		public static bool expandCodeExample = false;



		public bool HasTagGlobal (string tagToCheck)
		{
			
			
			
			foreach (var item in GlobalTagHolder.TagHolder.GlobalTagList.ToArray()) {
				
				
			if (string.Equals(item.Name,tagToCheck,StringComparison.CurrentCultureIgnoreCase))
				{
					
					return true;
					
				}
				
			}
			
			return false;
			
			
		}


		public override void OnInspectorGUI ()
		{ 
				MultiTags myScript = (MultiTags)target;

				
				EditorGUILayout.LabelField ("--------------------------------------------------------------------------------------------------------------------");
				GUI.color = Color.green;
				EditorGUILayout.LabelField ("LOCAL ASSIGNED TAGS: ");
				GUI.color = Color.white;
				GUILayout.Space (10);
				foreach (var item in myScript.localTagList.ToArray()) {

						EditorGUILayout.BeginHorizontal ();

						EditorGUILayout.LabelField ("Tag:   ", GUILayout.Width(40));

						//EditorGUILayout.TextField(item.Name);
				
						EditorGUILayout.SelectableLabel(item.Name, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
			GUI.color = Color.red + Color.yellow;
						if (GUILayout.Button ("Remove", GUILayout.Width (55))) {
								myScript.localTagList.Remove (item);
						}
			GUI.color = Color.white;
			 
						EditorGUILayout.EndHorizontal ();

					if (!HasTagGlobal(item.Name))
						{
						EditorGUILayout.BeginHorizontal ();
						GUI.color = Color.yellow;
						EditorGUILayout.LabelField ("warning: (" + item.Name + ") is not a global tag.",GUILayout.Height(25));
						GUI.color = Color.white;
						EditorGUILayout.EndHorizontal ();
						

						}

				}


				
				EditorGUILayout.LabelField ("--------------------------------------------------------------------------------------------------------------------");



				EditorGUILayout.LabelField ("--------------------------------------------------------------------------------------------------------------------");

					GUI.color = Color.cyan;
				expandGlobals = EditorGUILayout.Foldout (expandGlobals, "GLOBAL PROJECT TAGS:  ");
				GUI.color = Color.white;
				if (expandGlobals) {

			foreach (var itemG in GlobalTagHolder.TagHolder.GlobalTagList.ToArray()) {


								EditorGUILayout.BeginHorizontal ();

		
			
				if (myScript.gameObject.HasTag(itemG.Name))
				{
					GUI.color = Color.green;
					EditorGUILayout.LabelField ("Assigned  ", GUILayout.Width(55) );
					GUI.color = Color.white;
				}
				else
				{
					GUI.color = Color.green;
					//EditorGUILayout.LabelField ("Tag:  " + item);
					if (GUILayout.Button ("Assign", GUILayout.Width (55))) {
						
						myScript.localTagList.Add (itemG);
						
					}
					GUI.color = Color.white;

				}


						


						


				
				EditorGUILayout.LabelField ("Tag:   ", GUILayout.Width(40));			

			//	EditorGUILayout.LabelField ("Tag:  " + itemG.Name);

				EditorGUILayout.SelectableLabel(itemG.Name, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
				GUI.color = Color.red + Color.red;
				if (GUILayout.Button ("Destroy", GUILayout.Width (55))) {
					
					GlobalTagHolder.TagHolder.GlobalTagList.Remove (itemG);
					GlobalTagHolder.TagHolder.Save();
					
				}
				GUI.color = Color.white;
							//	EditorGUILayout.LabelField ("Description:  " + itemAll.Description.ToString (), GUILayout.MaxWidth (160));

								//item.Description = EditorGUILayout.TextField ("Description: ", item.Description, GUILayout.MaxWidth(120));

								//EditorGUILayout.Separator();
		
			
								EditorGUILayout.EndHorizontal ();

								EditorGUILayout.BeginHorizontal ();
							//	itemAll.ID = (byte)EditorGUILayout.IntField ("ReID: ", itemAll.ID, GUILayout.MaxWidth (180));
							//	itemAll.Name = EditorGUILayout.TextField ("Rename: ", itemAll.Name);



								EditorGUILayout.EndHorizontal ();
							//	EditorGUILayout.LabelField ("-----------");
			
						}
		

						EditorGUILayout.LabelField ("--------------------------------------------------------------------------------------------------------------------");

				}

				GUILayout.Space (20);
				EditorGUILayout.BeginHorizontal ();



				//tagDescription = GUILayout.TextField(tagDescription,20);
				tagDescription = EditorGUILayout.TextField ("Tag Name: ", tagDescription);
				GUI.color = Color.cyan;
				if (GUILayout.Button ("Add New Tag", GUILayout.Width (100))) {


						tagDescription = tagDescription.Trim();
						


							if (string.IsNullOrEmpty(tagDescription))
							{
									return;
								
							}
			 


			MT itemLocal = new MT ();
			MT itemGlobal = new MT ();
			
						itemLocal.Name = tagDescription;
					//	item.ID = 44;
					//	itemLocal.tagGUID = Guid.NewGuid().ToString();;

						itemGlobal.Name = itemLocal.Name;
					//	itemGlobal.tagGUID = itemLocal.tagGUID;
			
						
						
						


							if (!HasTagGlobal(tagDescription))
							{
								
								GlobalTagHolder.TagHolder.GlobalTagList.Add (itemGlobal);
								GlobalTagHolder.TagHolder.Save();
							
								
							}
							if (!myScript.gameObject.HasTag(tagDescription))
							{
								
								myScript.localTagList.Add (itemLocal);
								
								
							}

					tagDescription = string.Empty;

				}
		GUI.color = Color.white;
				EditorGUILayout.EndHorizontal ();
				GUILayout.Space (20);

		expandCodeExample = EditorGUILayout.Foldout (expandCodeExample, "CODE EXAMPLE:  ");
		
		if (expandCodeExample) {

			GUILayout.TextArea (stringToEdit, 200);

				}

				

				GUILayout.Space (20);
			//	DrawDefaultInspector ();


				if (GUI.changed) {
					//	Debug.Log ("I Changed");


						//	foreach (var item in myScript.multiTag) {

//				if (String.IsNullOrEmpty(item.TestGUID())) {
//					
//				item.SetGUID(Guid.NewGuid().ToString());
//				
//				}
				 
						//		}


				}

		}




}
