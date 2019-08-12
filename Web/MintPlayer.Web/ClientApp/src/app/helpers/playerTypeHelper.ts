import { Injectable } from "@angular/core";
import { ePlayerType } from "../enums/ePlayerType";
import { PlayerType } from "../interfaces/playerType";

@Injectable({
  providedIn: 'root'
})

export class PlayerTypeHelper {
  public getPlayerTypes() {
    var playerTypeValues = Object.values(ePlayerType);
    var playerTypes = playerTypeValues.slice(playerTypeValues.length / 2).map<PlayerType>((id) => {
      var id_num = <number>id;
      return { id: id_num, description: <string>playerTypeValues[id_num] };
    });
    return playerTypes;
  }
}
