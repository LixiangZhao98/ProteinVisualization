# ProteinVisualization

This is a Protein Visualization demo project built with Unity3D. It reads and visualizes .pdb files. The current visualizations includes space-filling, balls-and-sticks, licorice and backbone diagrams, and...the seamless transition between these diagrams. The repository is keeping to update.

## Visualization
<div align=center>
<img src="https://github.com/LixiangZhao98/ProteinVisualization/blob/master/Assets/my/pic/protein2.png" width="615" height="684"> <width="640" height="684"/>
</div>

<div align=center>
<img src="https://github.com/LixiangZhao98/ProteinVisualization/blob/master/Assets/my/pic/protein1.png" width="664" height="324"> <width="640" height="360"/>
</div>

## Seamless Transition
<div align=center>
<img src="https://github.com/LixiangZhao98/ProteinVisualization/blob/master/Assets/my/pic/transition.gif" width="640" height="360"> <width="640" height="360"/>
</div>


<div align=center>
<img src="https://github.com/LixiangZhao98/ProteinVisualization/blob/master/Assets/my/pic/transition2.gif" width="640" height="360"> <width="640" height="360"/>
</div>

# How to use
* Clone the repo with git lfs installed and open the project using Unity (versions 2020.3.38f1 has been tested).
* `Assets/my/Scenes/protein.unity` and `Assets/my/Scenes/proteinAnimation.unity` are the main scenes.
* After you run the program in the editor, you can switch the dataset by first clicking `script` gameobject in the hierarchy, then change the `Dataset` property in the inspector.

# Thanks
Many thanks to the authors of open-source repository:
[SplineMesh](https://github.com/methusalah/SplineMesh "SplineMesh")