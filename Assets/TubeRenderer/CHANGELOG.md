# Changelog

## [1.9.4] - 2020-10-19
- Added compatibility with URP and HDRP.
- Added GetLengthAtPoint().
- Eliminated minor garbage created when normals were recalculated.
- Adopted unity's semantic versioning.
- Adopted package default package layout file structure.
- Added new Invertebrate example.
- Updated Slide example to use compound colliders.

## [1.9.3] - 2019-10-03
- Fixed inspector foldout toggle issue in Unity 2019.3.
- Fixed occational twist at first point.

## [1.9.2] - 2018-12-13
- Added uvAntiStretching proptery. Enable to relax uvs along the tube when segments are different length.
- Fixed odd twist at first segment when setting point count in inspector.
- Fixed Example 08 Skin destroy order issue.
- Reduced heap garbage generation in update loop to zero.

## [1.9.1] - 2018-11-20
- Fixed null error occurring in combination with third party inspector extensions.
- Removed support for Unity versions below 2017.1.

## [1.9.0] - 2018-11-04
- Added the ability to apply a twist at each tube point.

## [1.8.0] - 2017-08-10
- Added a new SplineMaker component.
- Added an example scene for the SplineMaker component.
- Added length getter.
- Renamed meshGizmos to showMeshGizmos
- Added showRotationGizmos
- Added OnDidApplyAnimationProperties for compatibility with Unity animation.
- Fixed null error on Awake.
- Fixed minor error when using hard edges normal mode.

## [1.7.0] - 2016-12-20
- Replaced Unity error when vertex limit is exceeded with a harmless warning.
- Minor inspector improvements.
- Fixed problem with duplicated and pasted tubes in Editor.
- Changed Update to LateUpdate to increase the chance of properties being changed before the mesh is updated.
- Fixed dirtyColors flag not set to false when no colors are provided.
- Removed mesh.tangents check in Redraw method.
- Updated 01 Herd example.
- Changed deprecated Random.seed to Random.InitState().

## [1.6.0] - 2016-01-30
- Fixed serialization issue by implementing SerializedProperty and OnValidate correctly.
- Changed the default material from "Diffuse" to "Standard".
- Moved TubeRenderer directory from "Assets/Asset Store/TubeRenderer" to "Assets/TubeRenderer".
- Moved all documentation to the "TubeRenderer/Documents" directory.
- Changed format of Reference file from markdown to pdf.
- Removed obsolete methods. They have been obsolete since v. 1.2.
- Updated examples to use the new UI system.
- Code refactoring.

## [1.5.0] - 2015-09-18
- Fixed Unity5 warnings.
- Fixed bug related to serialisation of the radiuses array.
- Tubes created in the editor will now start out with two points.

## [1.4.0] - 2014-08-27
- Added 'Skin' example showing how to apply a SkinnedMeshRenderer to a tube.
- Fixed minor uv mapping bug for caps when using HardEdges.
- Optimised vector calculations (using Set).
- Optimised color updates and converted external colors to Color32.
- Added new method 'GetRotationAtPoint'.

## [1.3.0] - 2013-11-29
- Added 'normalMode': define how to render normals; Smooth, Hard or HardEdges.
- Updated a few of the examples.
- Optimised calculation of mesh data (even more).
- Renamed 'drawMeshGizmos' to 'meshGizmos'.
- Renamed 'drawMeshGizmosLength' to 'meshGizmoLength'.

## [1.2.0] - 2013-11-01
- Converted all methods for setting and getting tube data to properties.
- Added 'caps': open or close the end points of your tube.
- Added 'uvRect', 'uvRectCap' and 'uvRectCapEndMirrored': create custom uv mappings.
- Added 'forwardAngleOffset': rotate tube around it's forward direction.
- Added 'AddPostprocess', 'RemovePostprocess' and 'postprocessContinously': post process the mesh of your tube.
- Added 'MarkDynamic' shortcut to mesh.MarkDynamic(): optimise for runtime manipulation.
- Added 'drawMeshGizmos' and 'drawMeshGizmosLength': display gizmos for normals and tangents.
- Implemented modulo iteration for cases where the length of point, radius and color arrays don't match.
- Perfected normals and tangents for tubes with variable radiuses.
- Fixed minor update issue with 'calculateTangents' and 'invertMesh'.
- Fixed rotation issue with doubled points.
- Added custom inspector and enabled editor execution.
- Moved documentation to 'AssetStore/TubeRenderer/Reference.md' and updated it.
- Created remote documentation at 'http://sixthsensor.dk/code/unity/tuberenderer/'
- Created new examples and updated the old.

## [1.1.0] - 2013-06-25
- Fixed normal magnitude error.
- Added inline documentation.
- Changed Color to Color32 internally.
- Changed the 'EdgeCount' method to a 'edgeCount' property.
- Moved 'TubeRenderer' folder inside 'Asset Store' folder.

## [1.0.0] - 2012-04-01
- Initial asset store version.