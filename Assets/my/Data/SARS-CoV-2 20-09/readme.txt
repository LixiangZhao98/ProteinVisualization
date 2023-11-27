This README has to be updated for SARS-CoV-2 20-06!!!

This archive contains a model of SARS-CoV2 created as a part of a submission to VIS 2020.

Format of data:
- plain text format
- one protein model instance per line

Format of the line: "<model-name> x y z a b c d <sequence-id>" where:
- <model-name> is the name of PDB file within this archive
- x,y,z are coordinates of the protein model
- a,b,c,d is a rotation represented by a quaternion QQuaternion(-d,a,b,-c), where QQuaternion(s,x,y,z) is (https://doc.qt.io/qt-5/qquaternion.html)
- <sequence-id> if not lower than 0 represents the order in which the element was populated by a rule

Notes:
- every PDB model has to be centered first and scaled by the #scale factor
- the location x,y,z of the instance inside .txt file is given already after scaling


Examples: 
- first MD entry "MD -0.00518228 -0.131343 -0.0512778 0.335574 -0.651041 -0.680039 -0.0328925 -1" rotation is constructed as:
auto q = QQuaternion(-terms[7].toFloat(), terms[4].toFloat(), terms[5].toFloat(), -terms[6].toFloat())
q.toRotationMatrix() returns the rotation matrix
QGenericMatrix<3, 3, float>(
 -0.908522 0.0595023 -0.413581         
 -0.162878  0.861077  0.481682         
  0.384786  0.504982 -0.772615         
)

- MD.pdb model has center at [-64.1804962, 40.1474991, 32.9205017] and bounding sphere radius is 50.7542953. After scaling (#scale) the BS radius is 0.01522628.

Citation:
Modeling in the Time of COVID-19: Statistical and Rule-based Mesoscale Models
N. Nguyen, O. Strnad, T. Klein, D. Luo, R. Alharbi, P. Wonka, M. Maritan, P. Mindek, L. Autin, D. Goodsell, I. Viola: arXiv preprint, https://arxiv.org/abs/2005.01804, 2020