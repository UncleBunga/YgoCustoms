-- Slasher - Damballa
local s,id=GetID()
function s.initial_effect(c)
    -- Ritual Summon 1 "Slasher - Possessed Puppet" from your hand
    Ritual.AddProcGreater{
        handler=c,
        filter=function(c)
            return c:IsSetCard(0x991) and c:IsType(TYPE_RITUAL)
        end,
        -- Define the GY as an extra location for materials
        extrafil=function(e,tp)
            return Duel.GetMatchingGroup(aux.AND(Card.HasLevel,Card.IsAbleToRemove),tp,LOCATION_GRAVE,0,nil)
        end,
        -- Tell the game engine we might banish cards so it handles the prompts correctly
        extratg=function(e,tp,eg,ep,ev,re,r,rp,chk)
            if chk==0 then return true end
            Duel.SetOperationInfo(0,CATEGORY_REMOVE,nil,1,tp,LOCATION_GRAVE)
        end
    }
end