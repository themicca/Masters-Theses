export class Utils {
  static offSetOppositeLinks(graph: joint.dia.Graph) {
    graph.getLinks().forEach(link => {
      const oppositeLink = graph.getLinks().find(l =>
        l.getSourceElement() === link.getTargetElement() && l.getTargetElement() === link.getSourceElement()
      );
      if (!oppositeLink) return;

      console.log("step " + oppositeLink);
      const sourceCenter = link.getSourceElement()!.getBBox().center();
      const targetCenter = link.getTargetElement()!.getBBox().center();
      const midX = (sourceCenter.x + targetCenter.x) / 2;
      const midY = (sourceCenter.y + targetCenter.y) / 2;

      link.vertices([{ x: midX, y: midY + 20 }]);
      oppositeLink.vertices([{ x: midX, y: midY - 20 }]);
    });
  }
}
